using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Content;

public sealed class RoadSignCategory : Entity
{
    private readonly List<RoadSignCategoryTranslation> _translations = new();
    private readonly List<RoadSign> _signs = new();

    private RoadSignCategory() { }

    public int OrderIndex { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<RoadSignCategoryTranslation> Translations => _translations;
    public IReadOnlyCollection<RoadSign> Signs => _signs;

    public static RoadSignCategory Create(int orderIndex, IEnumerable<(string Lang, string Name)> translations)
    {
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));

        var category = new RoadSignCategory
        {
            Id = Guid.NewGuid(),
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        foreach (var (lang, name) in translations)
            category.UpsertTranslation(lang, name);

        if (category._translations.Count == 0)
            throw new InvalidOperationException("Category requires at least one translation.");

        return category;
    }

    public void SetOrder(int orderIndex)
    {
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));
        OrderIndex = orderIndex;
        Touch();
    }

    public void UpsertTranslation(string languageCode, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        var lang = Normalize(languageCode);

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == lang);
        if (existing is null)
            _translations.Add(RoadSignCategoryTranslation.Create(Id, lang, name));
        else
            existing.UpdateName(name);

        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static string Normalize(string lang) =>
        string.IsNullOrWhiteSpace(lang) ? throw new ArgumentException("Language code required.") : lang.ToLowerInvariant();
}
