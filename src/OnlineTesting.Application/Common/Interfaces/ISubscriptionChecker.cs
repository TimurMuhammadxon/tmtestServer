namespace OnlineTesting.Application.Common.Interfaces;

public interface ISubscriptionChecker
{
    Task<bool> IsActiveAsync(Guid userId, CancellationToken ct);
    Task<bool> IsTeacherSubscriptionActiveAsync(Guid userId, CancellationToken ct);
}