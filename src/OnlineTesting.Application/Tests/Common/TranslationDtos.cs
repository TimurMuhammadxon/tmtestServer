namespace OnlineTesting.Application.Tests.Common;

public record TopicTranslationDto(string LanguageCode, string Name);
public record QuestionTranslationDto(string LanguageCode, string Text, string? Explanation);
public record AnswerTranslationDto(string LanguageCode, string Text);

public record AnswerInputDto(int OrderIndex, bool IsCorrect, List<AnswerTranslationDto> Translations);