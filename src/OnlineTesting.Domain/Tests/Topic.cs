using System.Text.RegularExpressions;
using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Tests;

public sealed class Topic : Entity
{
    private static readonly Regex CodePattern = new("^[a-z0-9-]+$", RegexOptions.Compiled);

    private readonly List<TopicTranslation> _translations = new();

    private Topic() { }

    public string Code { get; private set; } = default!;
    public int OrderIndex { get; private set; }
    public bool IsDemo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<TopicTranslation> Translations => _translations;

    public static Topic Create(string code, int orderIndex, bool isDemo, IEnumerable<(string Lang, string Name)> translations)
    {
        ValidateCode(code);
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));

        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Code = code.ToLowerInvariant(),
            OrderIndex = orderIndex,
            IsDemo = isDemo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var (lang, name) in translations)
            topic.UpsertTranslation(lang, name);

        if (topic._translations.Count == 0)
            throw new InvalidOperationException("Topic requires at least one translation.");

        return topic;
    }

    public void UpdateBasics(string code, int orderIndex)
    {
        ValidateCode(code);
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));

        Code = code.ToLowerInvariant();
        OrderIndex = orderIndex;
        Touch();
    }

    public void SetDemo(bool isDemo)
    {
        IsDemo = isDemo;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void UpsertTranslation(string languageCode, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        var lang = Normalize(languageCode);

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == lang);
        if (existing is null)
            _translations.Add(TopicTranslation.Create(Id, lang, name));
        else
            existing.UpdateName(name);

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

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length is < 2 or > 50 || !CodePattern.IsMatch(code))
            throw new ArgumentException("Code must be 2..50 chars, lowercase, digits and hyphens.", nameof(code));
    }

    private static string Normalize(string lang) =>
        string.IsNullOrWhiteSpace(lang) ? throw new ArgumentException("Language code required.") : lang.ToLowerInvariant();
}