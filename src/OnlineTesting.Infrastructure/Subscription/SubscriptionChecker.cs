using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Infrastructure.Persistence;

namespace OnlineTesting.Infrastructure.Subscriptions;

public class SubscriptionChecker : ISubscriptionChecker
{
    private readonly ApplicationDbContext _db;

    public SubscriptionChecker(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsActiveAsync(Guid userId, CancellationToken ct) =>
        _db.Subscriptions.AnyAsync(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow, ct);

    public Task<bool> IsTeacherSubscriptionActiveAsync(Guid userId, CancellationToken ct) =>
        _db.Subscriptions
            .Join(_db.SubscriptionPlans, s => s.PlanId, p => p.Id, (s, p) => new { s, p })
            .AnyAsync(x => x.s.UserId == userId
                && x.s.ExpiresAt > DateTime.UtcNow
                && x.p.Type == Domain.Subscriptions.SubscriptionPlanType.Teacher, ct);
}
