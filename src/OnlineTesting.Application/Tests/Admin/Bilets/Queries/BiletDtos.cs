namespace OnlineTesting.Application.Tests.Admin.Bilets.Queries;

public record BiletListItemDto(
    Guid Id,
    int Number,
    bool IsDemo,
    bool IsActive,
    int QuestionsCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record BiletDetailsDto(
    Guid Id,
    int Number,
    bool IsDemo,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<BiletQuestionDto> Questions
);

public record BiletQuestionDto(
    int OrderIndex,
    Guid QuestionId,
    string? ImageKey,
    string Text,
    string? Explanation,
    string Language,
    bool IsFallback,
    IReadOnlyList<BiletAnswerDto> Answers
);

public record BiletAnswerDto(
    Guid Id,
    int OrderIndex,
    string Text,
    string Language,
    bool IsFallback,
    bool IsCorrect
);