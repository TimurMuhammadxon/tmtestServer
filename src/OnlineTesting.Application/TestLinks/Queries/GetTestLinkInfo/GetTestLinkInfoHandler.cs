using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.TestLinks.Queries.GetTestLinkInfo;

public class GetTestLinkInfoHandler : IRequestHandler<GetTestLinkInfoQuery, TestLinkInfoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ISubscriptionChecker _subscription;

    public GetTestLinkInfoHandler(IApplicationDbContext db, ICurrentUser currentUser, ISubscriptionChecker subscription)
    {
        _db = db;
        _currentUser = currentUser;
        _subscription = subscription;
    }

    public async Task<TestLinkInfoDto> Handle(GetTestLinkInfoQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == request.Code.ToUpper() && t.IsActive, ct)
            ?? throw new NotFoundException("Invalid or expired link.");

        if (link.ExpiresAt <= DateTime.UtcNow)
            throw new ConflictException("This link has expired.");

        if (!await _subscription.IsActiveAsync(userId, ct))
            throw new ConflictException("A paid subscription is required to use this link.");

        var attemptsUsed = await _db.Attempts
            .CountAsync(a => a.TestLinkId == link.Id && a.UserId == userId, ct);

        return new TestLinkInfoDto(
            link.Title,
            link.FlowType.ToString(),
            link.MaxAttempts,
            link.ExpiresAt,
            attemptsUsed);
    }
}
