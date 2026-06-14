namespace OnlineTesting.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Endpoint { get; init; } = "";
    public string AccessKey { get; init; } = "";
    public string SecretKey { get; init; } = "";
    public string BucketName { get; init; } = "";
    public bool UseHttps { get; init; } = false;
    public string? PublicUrl { get; init; }
    public string? LocalPath { get; init; }

    public string PublicBaseUrl =>
        PublicUrl ?? $"{(UseHttps ? "https" : "http")}://{Endpoint}/{BucketName}";
}
