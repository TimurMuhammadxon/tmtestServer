using OnlineTesting.Domain.Common;

namespace OnlineTesting.Domain.Content;

public sealed class RoadSign : Entity
{
    private readonly List<RoadSignTranslation> _translations = new();

    private RoadSign() { }

    public Guid CategoryId { get; private set; }
    public string? Code { get; private set; }
    public string? ImageKey { get; private set; }
    public int OrderIndex { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<RoadSignTranslation> Translations => _translations;

    public static RoadSign Create(Guid categoryId, string? code, int orderIndex,
        IEnumerable<(string Lang, string Name, string? Description)> translations)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required.", nameof(categoryId));
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));

        var sign = new RoadSign
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            OrderIndex = orderIndex,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        foreach (var (lang, name, description) in translations)
            sign.UpsertTranslation(lang, name, description);

        if (sign._translations.Count == 0)
            throw new InvalidOperationException("Sign requires at least one translation.");

        return sign;
    }

    public void UpdateBasics(Guid categoryId, string? code, int orderIndex)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required.", nameof(categoryId));
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex must be non-negative.", nameof(orderIndex));
        CategoryId = categoryId;
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        OrderIndex = orderIndex;
        Touch();
    }

    public void SetImageKey(string? imageKey)
    {
        ImageKey = string.IsNullOrWhiteSpace(imageKey) ? null : imageKey;
        Touch();
    }

    public void Activate() { IsActive = true; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }

    public void UpsertTranslation(string languageCode, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        var lang = Normalize(languageCode);

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == lang);
        if (existing is null)
            _translations.Add(RoadSignTranslation.Create(Id, lang, name, description));
        else
            existing.Update(name, description);

        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static string Normalize(string lang) =>
        string.IsNullOrWhiteSpace(lang) ? throw new ArgumentException("Language code required.") : lang.ToLowerInvariant();
}
