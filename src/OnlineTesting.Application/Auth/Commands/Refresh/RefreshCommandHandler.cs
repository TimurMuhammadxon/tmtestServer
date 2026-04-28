using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Auth.Commands.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthResponse>
{
    private const string InvalidToken = "Invalid or expired refresh token.";
    private const string ReuseDetected = "Refresh token reuse detected. All sessions revoked.";

    private readonly IApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly IRequestContext _requestContext;

    public RefreshCommandHandler(
        IApplicationDbContext db,
        IJwtService jwt,
        IRefreshTokenService refresh,
        IRequestContext requestContext)
    {
        _db = db;
        _jwt = jwt;
        _refresh = refresh;
        _requestContext = requestContext;
    }

    public async Task<AuthResponse> Handle(RefreshCommand request, CancellationToken ct)
    {
        var incomingHash = _refresh.Hash(request.RefreshToken);

        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == incomingHash, ct);

        if (token is null)
            throw new UnauthorizedException(InvalidToken);

        // Replay attack: токен был использован ранее → отзываем всю активную цепочку.
        if (token.RevokedAt is not null)
        {
            await RevokeAllUserTokensAsync(token.UserId, ct);
            throw new UnauthorizedException(ReuseDetected);
        }

        if (DateTime.UtcNow >= token.ExpiresAt)
            throw new UnauthorizedException(InvalidToken);

        if (!token.User.IsActive)
            throw new UnauthorizedException(InvalidToken);

        // Rotation
        var (newRaw, newHash) = _refresh.Generate();
        var newToken = RefreshToken.Issue(
            token.UserId,
            newHash,
            DateTime.UtcNow.Add(_refresh.Lifetime),
            _requestContext.IpAddress);

        token.Revoke(replacedByTokenHash: newHash);
        _db.RefreshTokens.Add(newToken);

        var accessToken = _jwt.GenerateAccessToken(token.User);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Параллельный refresh уже отозвал этот токен → расцениваем как replay.
            await RevokeAllUserTokensAsync(token.UserId, ct);
            throw new UnauthorizedException(ReuseDetected);
        }

        return new AuthResponse(accessToken, newRaw, _jwt.AccessTokenExpirationSeconds);
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var t in activeTokens)
            t.Revoke();

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Другой запрос уже отозвал — это и есть желаемый результат.
        }
    }
}