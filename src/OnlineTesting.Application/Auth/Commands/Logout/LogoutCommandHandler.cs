using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IRefreshTokenService _refresh;
    private readonly ICurrentUser _currentUser;

    public LogoutCommandHandler(
        IApplicationDbContext db,
        IRefreshTokenService refresh,
        ICurrentUser currentUser)
    {
        _db = db;
        _refresh = refresh;
        _currentUser = currentUser;
    }

    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedException();

        var hash = _refresh.Hash(request.RefreshToken);

        var token = await _db.RefreshTokens.FirstOrDefaultAsync(
            t => t.TokenHash == hash && t.UserId == _currentUser.UserId, ct);

        // Идемпотентно: токен может быть отсутствующим/уже отозванным.
        if (token is { RevokedAt: null })
        {
            token.Revoke();
            await _db.SaveChangesAsync(ct);
        }
    }
}