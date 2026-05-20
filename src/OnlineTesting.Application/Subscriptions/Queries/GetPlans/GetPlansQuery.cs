using MediatR;

namespace OnlineTesting.Application.Subscriptions.Queries.GetPlans;

public record GetPlansQuery(bool AdminView = false) : IRequest<List<SubscriptionPlanDto>>;

public record SubscriptionPlanDto(
    Guid Id,
    string Type,
    string Duration,
    decimal Price,
    bool IsActive
);
