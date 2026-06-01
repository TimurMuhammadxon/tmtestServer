using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Users.Commands.SetCredentials;

public class SetCredentialsCommandHandler : IRequestHandler<SetCredentialsCommand, AuthResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly IRequestContext _requestContext;
    private readonly IDbExceptionInspector _dbInspector;

    public SetCredentialsCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IPasswordHasher hasher,
        IJwtService jwt,
        IRefreshTokenService refresh,
        IRequestContext requestContext,
        IDbExceptionInspector dbInspector)
    {
        _db = db;
        _currentUser = currentUser;
        _hasher = hasher;
        _jwt = jwt;
        _refresh = refresh;
        _requestContext = requestContext;
        _dbInspector = dbInspector;
    }

    public async Task<AuthResponse> Handle(SetCredentialsCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User '{userId}' not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Проверяем что новый email не занят другим пользователем
        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == normalizedEmail && u.Id != userId, ct);

        if (emailTaken)
            throw new ConflictException("This email is already registered.");

        var passwordHash = await _hasher.HashAsync(request.Password, ct);
        user.SetCredentials(normalizedEmail, passwordHash);

        var (raw, hash) = _refresh.Generate();
        var token = RefreshToken.Issue(
            user.Id,
            hash,
            DateTime.UtcNow.Add(_refresh.Lifetime),
            _requestContext.IpAddress);

        _db.RefreshTokens.Add(token);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException("This email is already registered.");
        }

        var accessToken = _jwt.GenerateAccessToken(user);
        return new AuthResponse(accessToken, raw, _jwt.AccessTokenExpirationSeconds);
    }
}
