using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Subscriptions.Queries.GetMySubscription;

public class GetMySubscriptionHandler : IRequestHandler<GetMySubscriptionQuery, MySubscriptionDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMySubscriptionHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MySubscriptionDto?> Handle(GetMySubscriptionQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var result = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Join(_db.SubscriptionPlans, s => s.PlanId, p => p.Id, (s, p) => new { s, p })
            .Select(x => new MySubscriptionDto(
                x.s.Id,
                x.p.Type.ToString(),
                x.p.Duration.ToString(),
                x.p.Price,
                x.s.StartsAt,
                x.s.ExpiresAt,
                x.s.ExpiresAt > DateTime.UtcNow))
            .FirstOrDefaultAsync(ct);

        return result;
    }
}
