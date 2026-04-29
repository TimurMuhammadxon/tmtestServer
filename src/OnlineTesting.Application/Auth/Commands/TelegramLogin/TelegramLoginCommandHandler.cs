using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Auth.Commands.TelegramLogin;

public class TelegramLoginCommandHandler : IRequestHandler<TelegramLoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IExternalAuthValidator _validator;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly IRequestContext _requestContext;
    private readonly IDbExceptionInspector _dbInspector;

    public TelegramLoginCommandHandler(
        IApplicationDbContext db,
        IExternalAuthValidator validator,
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

    public async Task<AuthResponse> Handle(TelegramLoginCommand request, CancellationToken ct)
    {
        // Шаг 1: Валидация подписи Telegram
        var authData = await _validator.ValidateTelegramAsync(request.InitData, ct);

        // Шаг 2: Поиск или создание юзера
        var user = await FindOrCreateUserAsync(authData, ct);

        if (!user.IsActive)
            throw new UnauthorizedException("User is inactive.");

        // Шаг 3: Выдача токенов (как при обычном login)
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

    private async Task<User> FindOrCreateUserAsync(
        Common.Models.TelegramAuthData authData,
        CancellationToken ct)
    {
        var existing = await _db.ExternalLogins
            .Include(e => e.User)
            .FirstOrDefaultAsync(e =>
                e.Provider == ExternalLoginProvider.Telegram &&
                e.ExternalUserId == authData.ExternalUserId, ct);

        if (existing is not null)
            return existing.User;

        // Юзер пришёл впервые — создаём
        var placeholderEmail = $"tg_{authData.ExternalUserId}@telegram.local";
        var user = User.CreateFromExternal(placeholderEmail, Role.Student);

        var external = ExternalLogin.Link(
            user.Id,
            ExternalLoginProvider.Telegram,
            authData.ExternalUserId,
            authData.Username);

        _db.Users.Add(user);
        _db.ExternalLogins.Add(external);

        try
        {
            await _db.SaveChangesAsync(ct);
            return user;
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            // Race: параллельный запрос успел создать. Перечитываем.
            var raceWinner = await _db.ExternalLogins
                .Include(e => e.User)
                .FirstOrDefaultAsync(e =>
                    e.Provider == ExternalLoginProvider.Telegram &&
                    e.ExternalUserId == authData.ExternalUserId, ct);

            if (raceWinner is null)
            {
                // Странный конфликт (например, по email). Не должно происходить.
                throw new UnauthorizedException("Authentication failed due to a conflict.");
            }

            return raceWinner.User;
        }
    }
}