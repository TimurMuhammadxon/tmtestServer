using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Subscriptions;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.GrantSubscription;

public class GrantSubscriptionHandler : IRequestHandler<GrantSubscriptionCommand, GrantSubscriptionResult>
{
    private readonly IApplicationDbContext _db;

    public GrantSubscriptionHandler(IApplicationDbContext db) => _db = db;

    public async Task<GrantSubscriptionResult> Handle(GrantSubscriptionCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException($"User '{request.UserId}' not found.");

        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, ct)
            ?? throw new NotFoundException($"Subscription plan '{request.PlanId}' not found.");

        var existing = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, ct);

        var baseDate = existing != null && existing.ExpiresAt > DateTime.UtcNow
            ? existing.ExpiresAt
            : DateTime.UtcNow;

        var newExpiresAt = ComputeExpiresAt(baseDate, plan.Duration);

        Guid subId;
        if (existing is null)
        {
            var sub = Subscription.Create(request.UserId, plan.Id, newExpiresAt);
            _db.Subscriptions.Add(sub);
            subId = sub.Id;
        }
        else
        {
            existing.Extend(plan.Id, newExpiresAt);
            subId = existing.Id;
        }

        if (plan.Type == SubscriptionPlanType.Teacher && user.Role == Role.Student)
            user.SetRole(Role.Teacher);

        await _db.SaveChangesAsync(ct);

        return new GrantSubscriptionResult(subId, newExpiresAt);
    }

    private static DateTime ComputeExpiresAt(DateTime baseDate, SubscriptionDuration duration) => duration switch
    {
        SubscriptionDuration.TwoWeeks    => baseDate.AddDays(14),
        SubscriptionDuration.OneMonth    => baseDate.AddMonths(1),
        SubscriptionDuration.TwoMonths   => baseDate.AddMonths(2),
        SubscriptionDuration.ThreeMonths => baseDate.AddMonths(3),
        _ => throw new ArgumentOutOfRangeException(nameof(duration))
    };
}
