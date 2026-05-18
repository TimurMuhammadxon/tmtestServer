namespace OnlineTesting.Application.Tests.Solutions.Attempts.Queries;

public record AttemptDto(
    Guid Id,
    string FlowType,
    string Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    int? CorrectCount,
    int TotalQuestions,
    int? RemainingSeconds,
    IReadOnlyList<AttemptQuestionDto> Questions);

public record AttemptQuestionDto(
    int OrderIndex,
    Guid QuestionId,
    string? ImageKey,
    string Text,
    string Language,
    bool IsFallback,
    Guid? ChosenAnswerId,
    bool? IsCorrect,
    DateTime? AnsweredAt,
    IReadOnlyList<AttemptAnswerDto> Answers);

public record AttemptAnswerDto(
    Guid Id,
    int OrderIndex,
    string Text,
    string Language,
    bool IsFallback,
    bool IsCorrect);
