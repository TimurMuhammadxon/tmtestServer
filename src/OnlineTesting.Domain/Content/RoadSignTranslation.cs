namespace OnlineTesting.Domain.Content;

public sealed class RoadSignTranslation
{
    private RoadSignTranslation() { }

    public Guid SignId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    internal static RoadSignTranslation Create(Guid signId, string languageCode, string name, string? description) =>
        new() { SignId = signId, LanguageCode = languageCode, Name = name, Description = description };

    internal void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
