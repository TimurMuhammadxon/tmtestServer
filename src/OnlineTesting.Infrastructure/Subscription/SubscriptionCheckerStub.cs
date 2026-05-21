using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Subscriptions;

public class SubscriptionCheckerStub : ISubscriptionChecker
{
    public Task<bool> IsActiveAsync(Guid userId, CancellationToken ct) => Task.FromResult(true);
    public Task<bool> IsTeacherSubscriptionActiveAsync(Guid userId, CancellationToken ct) => Task.FromResult(true);
}