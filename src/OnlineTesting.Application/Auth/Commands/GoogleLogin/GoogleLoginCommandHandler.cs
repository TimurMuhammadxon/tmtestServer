using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IGoogleAuthValidator _validator;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly IRequestContext _requestContext;
    private readonly IDbExceptionInspector _dbInspector;

    public GoogleLoginCommandHandler(
        IApplicationDbContext db,
        IGoogleAuthValidator validator,
        IJwtService jwt,
        IRefreshTokenService refresh,
        IRequestContext requestContext,
        IDbExceptionInspector dbInspector)
    {
        _db = db;
        _validator = validator;
        _jwt = jwt;
        _refresh = refresh;
        _requestContext = requestContext;
        _dbInspector = dbInspector;
    }

    public async Task<AuthResponse> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        var authData = await _validator.ValidateAsync(request.IdToken, ct);

        var user = await FindOrCreateUserAsync(authData, ct);

        if (!user.IsActive)
            throw new UnauthorizedException("User is inactive.");

        var accessToken = _jwt.GenerateAccessToken(user);

        var (raw, hash) = _refresh.Generate();
        var token = RefreshToken.Issue(
            user.Id,
            hash,
            DateTime.UtcNow.Add(_refresh.Lifetime),
            _requestContext.IpAddress);

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, raw, _jwt.AccessTokenExpirationSeconds);
    }

    private async Task<User> FindOrCreateUserAsync(GoogleAuthData authData, CancellationToken ct)
    {
        // Уже входил через Google раньше
        var existing = await _db.ExternalLogins
            .Include(e => e.User)
            .FirstOrDefaultAsync(e =>
                e.Provider == ExternalLoginProvider.Google &&
                e.ExternalUserId == authData.ExternalUserId, ct);

        if (existing is not null)
            return existing.User;

        // Email уже зарегистрирован (email/password) — привязываем Google к существующему аккаунту
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == authData.Email, ct);

        if (existingUser is not null)
        {
            // Обновим имя если ещё не заполнено
            if (existingUser.FirstName is null && existingUser.LastName is null)
                existingUser.SetName(authData.FirstName, authData.LastName);

            var link = ExternalLogin.Link(
                existingUser.Id,
                ExternalLoginProvider.Google,
                authData.ExternalUserId,
                authData.FirstName);

            _db.ExternalLogins.Add(link);
            await _db.SaveChangesAsync(ct);
            return existingUser;
        }

        // Новый пользователь
        var user = User.CreateFromExternal(authData.Email, Role.Student,
            authData.FirstName, authData.LastName);

        var external = ExternalLogin.Link(
            user.Id,
            ExternalLoginProvider.Google,
            authData.ExternalUserId,
            authData.FirstName);

        _db.Users.Add(user);
        _db.ExternalLogins.Add(external);

        try
        {
            await _db.SaveChangesAsync(ct);
            return user;
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            var raceWinner = await _db.ExternalLogins
                .Include(e => e.User)
                .FirstOrDefaultAsync(e =>
                    e.Provider == ExternalLoginProvider.Google &&
                    e.ExternalUserId == authData.ExternalUserId, ct);

            return raceWinner?.User
                ?? throw new UnauthorizedException("Authentication failed due to a conflict.");
        }
    }
}
