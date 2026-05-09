using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Tests;

public sealed class Answer : Entity
{
    private readonly List<AnswerTranslation> _translations = new();

    private Answer() { }

    public Guid QuestionId { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsCorrect { get; private set; }

    public IReadOnlyCollection<AnswerTranslation> Translations => _translations;

    internal static Answer Create(Guid questionId, int orderIndex, bool isCorrect)
    {
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));

        return new Answer
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            OrderIndex = orderIndex,
            IsCorrect = isCorrect
        };
    }

    internal void UpsertTranslation(string languageCode, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.", nameof(text));
        var lang = languageCode.ToLowerInvariant();

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == lang);
        if (existing is null)
            _translations.Add(AnswerTranslation.Create(Id, lang, text));
        else
            existing.UpdateText(text);
    }
}