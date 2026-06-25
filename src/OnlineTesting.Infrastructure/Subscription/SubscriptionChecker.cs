using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Infrastructure.Persistence;

namespace OnlineTesting.Infrastructure.Subscriptions;

public class SubscriptionChecker : ISubscriptionChecker
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public SubscriptionChecker(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> IsActiveAsync(Guid userId, CancellationToken ct)
    {
        var key = $"sub:active:{userId}";
        if (_cache.TryGetValue(key, out bool cached))
            return cached;

        var result = await _db.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow, ct);

        _cache.Set(key, result, CacheDuration);
        return result;
    }

    public async Task<bool> IsTeacherSubscriptionActiveAsync(Guid userId, CancellationToken ct)
    {
        var key = $"sub:teacher:{userId}";
        if (_cache.TryGetValue(key, out bool cached))
            return cached;

        var result = await _db.Subscriptions
            .Join(_db.SubscriptionPlans, s => s.PlanId, p => p.Id, (s, p) => new { s, p })
            .AnyAsync(x => x.s.UserId == userId
                && x.s.ExpiresAt > DateTime.UtcNow
                && x.p.Type == Domain.Subscriptions.SubscriptionPlanType.Teacher, ct);

        _cache.Set(key, result, CacheDuration);
        return result;
    }
}
