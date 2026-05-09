namespace OnlineTesting.Domain.Tests;

public sealed class Language
{
    private Language() { }

    public string Code { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    public static Language Create(string code, string displayName, bool isDefault, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Language code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));

        return new Language
        {
            Code = code.ToLowerInvariant(),
            DisplayName = displayName,
            IsDefault = isDefault,
            IsActive = isActive
        };
    }
}