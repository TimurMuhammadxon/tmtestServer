using MediatR;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Queries.GetAttempt;

public record GetAttemptQuery(Guid AttemptId) : IRequest<AttemptDto>;
