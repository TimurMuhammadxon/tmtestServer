namespace OnlineTesting.Application.Common.Interfaces;

public interface ISubscriptionChecker
{
    Task<bool> IsActiveAsync(Guid userId, CancellationToken ct);
}