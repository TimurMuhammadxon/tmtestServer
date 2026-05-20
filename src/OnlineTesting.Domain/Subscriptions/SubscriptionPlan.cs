using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Subscriptions;

public class SubscriptionPlan : Entity
{
    public SubscriptionPlanType Type { get; private set; }
    public SubscriptionDuration Duration { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(SubscriptionPlanType type, SubscriptionDuration duration) =>
        new() { Id = Guid.NewGuid(), Type = type, Duration = duration, Price = 0m, IsActive = true };

    public void SetPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        Price = price;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
