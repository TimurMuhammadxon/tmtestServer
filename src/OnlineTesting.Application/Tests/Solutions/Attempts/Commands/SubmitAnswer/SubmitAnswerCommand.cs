using MediatR;

namespace OnlineTesting.Application.Tests.Solutions.Attempts.Commands.SubmitAnswer;

public record SubmitAnswerCommand(Guid AttemptId, Guid QuestionId, Guid AnswerId)
    : IRequest<SubmitAnswerResult>;

public record SubmitAnswerResult(
    bool IsCorrect,
    Guid CorrectAnswerId,
    bool IsFinished,
    string Status,
    int? CorrectCount,
    int TotalQuestions);
