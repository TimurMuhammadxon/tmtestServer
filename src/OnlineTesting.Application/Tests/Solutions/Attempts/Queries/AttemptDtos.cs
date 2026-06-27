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
    bool ShowExplanations,
    IReadOnlyList<AttemptQuestionDto> Questions);

public record AttemptQuestionDto(
    int OrderIndex,
    Guid QuestionId,
    string? ImageUrl,
    string Text,
    string Language,
    bool IsFallback,
    Guid? ChosenAnswerId,
    bool? IsCorrect,
    DateTime? AnsweredAt,
    string? Explanation,
    IReadOnlyList<AttemptAnswerDto> Answers);

public record AttemptAnswerDto(
    Guid Id,
    int OrderIndex,
    string Text,
    string Language,
    bool IsFallback,
    bool IsCorrect);
