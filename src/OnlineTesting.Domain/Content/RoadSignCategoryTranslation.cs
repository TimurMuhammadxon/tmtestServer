namespace OnlineTesting.Domain.Content;

public sealed class RoadSignCategoryTranslation
{
    private RoadSignCategoryTranslation() { }

    public Guid CategoryId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    internal static RoadSignCategoryTranslation Create(Guid categoryId, string languageCode, string name) =>
        new() { CategoryId = categoryId, LanguageCode = languageCode, Name = name };

    internal void UpdateName(string name) => Name = name;
}
