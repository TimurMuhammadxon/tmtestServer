using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Subscriptions;

public class Subscription : Entity
{
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private Subscription() { }

    public static Subscription Create(Guid userId, Guid planId, DateTime expiresAt) =>
        new() { Id = Guid.NewGuid(), UserId = userId, PlanId = planId, StartsAt = DateTime.UtcNow, ExpiresAt = expiresAt };

    public void Extend(Guid planId, DateTime newExpiresAt)
    {
        PlanId = planId;
        ExpiresAt = newExpiresAt;
    }
}
