using MediatR;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.GrantSubscription;

public record GrantSubscriptionCommand(Guid UserId, Guid PlanId) : IRequest<GrantSubscriptionResult>;

public record GrantSubscriptionResult(Guid SubscriptionId, DateTime ExpiresAt);
