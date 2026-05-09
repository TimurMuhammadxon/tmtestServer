namespace OnlineTesting.Domain.Tests;

public sealed class TopicTranslation
{
    private TopicTranslation() { }

    public Guid TopicId { get; private set; }
    public string LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    internal static TopicTranslation Create(Guid topicId, string languageCode, string name) =>
        new() { TopicId = topicId, LanguageCode = languageCode, Name = name };

    internal void UpdateName(string name) => Name = name;
}