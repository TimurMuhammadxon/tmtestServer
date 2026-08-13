using MediatR;

namespace OnlineTesting.Application.Subscriptions.Queries.GetMySubscription;

public record GetMySubscriptionQuery : IRequest<MySubscriptionDto?>;

public record MySubscriptionDto(
    Guid SubscriptionId,
    string PlanType,
    string PlanDuration,
    decimal PlanPrice,
    DateTime StartsAt,
    DateTime ExpiresAt,
    bool IsActive,
    bool IsTrial = false
);
