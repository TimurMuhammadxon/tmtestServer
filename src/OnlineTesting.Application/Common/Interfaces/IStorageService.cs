namespace OnlineTesting.Application.Common.Interfaces;

public interface IStorageService
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
    string GetPublicUrl(string key);
}
