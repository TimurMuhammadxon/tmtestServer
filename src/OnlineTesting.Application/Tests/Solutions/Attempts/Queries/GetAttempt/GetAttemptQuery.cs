using MediatR;
using OnlineTesting.Application.Tests.Solutions.Attempts.Queries;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Queries.GetAttempt;

public record GetAttemptQuery(Guid AttemptId) : IRequest<AttemptDto>;
