using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Storage;

public class FileSystemStorageService : IStorageService
{
    private readonly StorageOptions _options;

    public FileSystemStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    private string FullPath(string key) =>
        Path.Combine(_options.LocalPath!, key.Replace('/', Path.DirectorySeparatorChar));

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var path = FullPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var fs = File.Create(path);
        await content.CopyToAsync(fs, ct);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var path = FullPath(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key) => $"{_options.PublicBaseUrl}/{key}";
}
