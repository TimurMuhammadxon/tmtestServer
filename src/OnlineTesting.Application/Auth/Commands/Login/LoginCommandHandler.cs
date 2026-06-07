using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private const string InvalidCredentials = "Invalid credentials.";

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly IRequestContext _requestContext;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher hasher,
        IJwtService jwt,
        IRefreshTokenService refresh,
        IRequestContext requestContext)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _refresh = refresh;
        _requestContext = requestContext;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Constant-time defense: всегда выполняем BCrypt.Verify.
        // Учитываем три случая:
        //   - user == null → используем DummyHash
        //   - user существует, но PasswordHash == null (Telegram-only юзер) → используем DummyHash
        //   - user существует с реальным хешем → используем его
        // Все три случая занимают одинаковое время.
        var passwordHash = user?.PasswordHash ?? _hasher.DummyHash;
        var passwordValid = await _hasher.VerifyAsync(request.Password, passwordHash, ct);

        if (user is null || !user.IsActive || user.PasswordHash is null || !passwordValid)
            throw new UnauthorizedException(InvalidCredentials);

        var accessToken = _jwt.GenerateAccessToken(user);

        var oldTokens = await _db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var old in oldTokens)
            old.Revoke();

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
}