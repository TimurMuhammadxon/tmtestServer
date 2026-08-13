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
                x.s.ExpiresAt > DateTime.UtcNow,
                false))
            .FirstOrDefaultAsync(ct);

        if (result is not null)
            return result;

        // No subscription row → free trial during the first 24h after registration.
        var now = DateTime.UtcNow;
        var createdAt = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (DateTime?)u.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (createdAt is not null)
        {
            var trialEnd = createdAt.Value.AddDays(1);
            if (trialEnd > now)
                return new MySubscriptionDto(
                    Guid.Empty, "Trial", "", 0m, createdAt.Value, trialEnd, IsActive: true, IsTrial: true);
        }

        return null;
    }
}
