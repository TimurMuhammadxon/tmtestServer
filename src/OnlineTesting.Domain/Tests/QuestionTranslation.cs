namespace OnlineTesting.Domain.Tests;

public sealed class QuestionTranslation
{
    private QuestionTranslation() { }

    public Guid QuestionId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string Text { get; private set; } = default!;
    public string? Explanation { get; private set; }

    internal static QuestionTranslation Create(Guid questionId, string languageCode, string text, string? explanation) =>
        new()
        {
            QuestionId = questionId,
            LanguageCode = languageCode,
            Text = text,
            Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation
        };

    internal void Update(string text, string? explanation)
    {
        Text = text;
        Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation;
    }
}