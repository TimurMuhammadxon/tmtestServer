namespace OnlineTesting.Domain.Tests;

public sealed class AnswerTranslation
{
    private AnswerTranslation() { }

    public Guid AnswerId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string Text { get; private set; } = default!;

    internal static AnswerTranslation Create(Guid answerId, string languageCode, string text) =>
        new() { AnswerId = answerId, LanguageCode = languageCode, Text = text };

    internal void UpdateText(string text) => Text = text;
}