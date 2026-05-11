using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Subscription;

public class SubscriptionCheckerStub : ISubscriptionChecker
{
    public Task<bool> IsActiveAsync(Guid userId, CancellationToken ct) => Task.FromResult(true);
}