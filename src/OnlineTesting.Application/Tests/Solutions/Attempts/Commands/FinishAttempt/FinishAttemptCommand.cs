using MediatR;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.FinishAttempt;

public record FinishAttemptCommand(Guid AttemptId) : IRequest<FinishAttemptResult>;

public record FinishAttemptResult(
    string Status,
    int CorrectCount,
    int TotalQuestions,
    DateTime FinishedAt);
