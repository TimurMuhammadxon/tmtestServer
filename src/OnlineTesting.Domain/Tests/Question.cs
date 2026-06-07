using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Tests;

public sealed class Question : Entity
{
    public const int MinAnswers = 2;
    public const int MaxAnswers = 6;

    private readonly List<Answer> _answers = new();
    private readonly List<QuestionTranslation> _translations = new();

    private Question() { }

    public Guid TopicId { get; private set; }
    public string? ImageKey { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Answer> Answers => _answers;
    public IReadOnlyCollection<QuestionTranslation> Translations => _translations;

    public static Question Create(
        Guid topicId,
        string? imageKey,
        IEnumerable<(string Lang, string Text, string? Explanation)> translations,
        IEnumerable<AnswerDraft> answers)
    {
        if (topicId == Guid.Empty)
            throw new ArgumentException("TopicId is required.", nameof(topicId));

        var q = new Question
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            ImageKey = string.IsNullOrWhiteSpace(imageKey) ? null : imageKey,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var (lang, text, explanation) in translations)
            q.UpsertTranslation(lang, text, explanation);

        foreach (var draft in answers)
            q.AddAnswerInternal(draft);

        q.ValidateAnswers();

        if (q._translations.Count == 0)
            throw new InvalidOperationException("Question requires at least one translation.");

        return q;
    }

    public void UpdateBasics(Guid topicId)
    {
        if (topicId == Guid.Empty)
            throw new ArgumentException("TopicId is required.", nameof(topicId));
        TopicId = topicId;
        Touch();
    }

    public void SetImage(string key) { ImageKey = key; Touch(); }
    public void RemoveImage() { ImageKey = null; Touch(); }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }

    public void UpsertTranslation(string languageCode, string text, string? explanation)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.", nameof(text));
        var lang = Normalize(languageCode);

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == lang);
        if (existing is null)
            _translations.Add(QuestionTranslation.Create(Id, lang, text, explanation));
        else
            existing.Update(text, explanation);

        Touch();
    }

    public void RemoveTranslation(string languageCode)
    {
        var lang = Normalize(languageCode);
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == lang)
            ?? throw new InvalidOperationException($"Translation '{lang}' not found.");
        if (_translations.Count == 1)
            throw new InvalidOperationException("Cannot remove the last translation.");
        _translations.Remove(existing);
        Touch();
    }

    /// <summary>
    /// Replace the full set of answers (used on PUT /admin/questions/{id}).
    /// </summary>
    public void ReplaceAnswers(IEnumerable<AnswerDraft> drafts)
    {
        _answers.Clear();
        foreach (var draft in drafts)
            AddAnswerInternal(draft);
        ValidateAnswers();
        Touch();
    }

    private void AddAnswerInternal(AnswerDraft draft)
    {
        var answer = Answer.Create(Id, draft.OrderIndex, draft.IsCorrect);
        foreach (var (lang, text) in draft.Translations)
            answer.UpsertTranslation(lang, text);

        if (answer.Translations.Count == 0)
            throw new InvalidOperationException("Each answer requires at least one translation.");

        _answers.Add(answer);
    }

    private void ValidateAnswers()
    {
        if (_answers.Count is < MinAnswers or > MaxAnswers)
            throw new InvalidOperationException($"Question must have {MinAnswers}..{MaxAnswers} answers.");

        if (_answers.Count(a => a.IsCorrect) != 1)
            throw new InvalidOperationException("Question must have exactly one correct answer.");

        var orders = _answers.Select(a => a.OrderIndex).ToList();
        if (orders.Distinct().Count() != orders.Count)
            throw new InvalidOperationException("Answer OrderIndex values must be unique.");
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static string Normalize(string lang) =>
        string.IsNullOrWhiteSpace(lang) ? throw new ArgumentException("Language code required.") : lang.ToLowerInvariant();
}

public readonly record struct AnswerDraft(
    int OrderIndex,
    bool IsCorrect,
    IReadOnlyList<(string Lang, string Text)> Translations);