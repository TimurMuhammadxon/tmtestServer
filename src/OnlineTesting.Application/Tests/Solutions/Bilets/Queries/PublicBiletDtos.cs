namespace OnlineTesting.Application.Tests.Solutions.Bilets.Queries;

public record PublicBiletListItemDto(
    Guid Id,
    int Number,
    bool IsDemo,
    int QuestionCount
);

public record PublicBiletDetailsDto(
    Guid Id,
    int Number,
    bool IsDemo,
    IReadOnlyList<PublicBiletQuestionDto> Questions
);

public record PublicBiletQuestionDto(
    int OrderIndex,
    Guid QuestionId,
    string? ImageKey,
    string Text,
    string? Explanation,
    string Language,
    bool IsFallback,
    IReadOnlyList<PublicBiletAnswerDto> Answers
);

public record PublicBiletAnswerDto(
    Guid Id,
    int OrderIndex,
    string Text,
    string Language,
    bool IsFallback,
    bool IsCorrect
);