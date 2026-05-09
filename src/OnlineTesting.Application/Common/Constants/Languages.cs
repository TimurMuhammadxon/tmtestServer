namespace OnlineTesting.Application.Common.Constants;

public static class Languages
{
    public const string Default = UzLatn;

    public const string UzLatn = "uz-latn";
    public const string Ru = "ru";
    public const string UzCyrl = "uz-cyrl";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        UzLatn, Ru, UzCyrl
    };

    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code) && All.Contains(code);
}