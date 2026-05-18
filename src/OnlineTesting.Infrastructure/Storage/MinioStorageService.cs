using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly StorageOptions _options;

    public MinioStorageService(IAmazonS3 s3, IOptions<StorageOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            UseChunkEncoding = false
        };

        await _s3.PutObjectAsync(request, ct);
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        try
        {
            await _s3.DeleteObjectAsync(_options.BucketName, key, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // file already gone — not an error
        }
    }

    public string GetPublicUrl(string key) => $"{_options.PublicBaseUrl}/{key}";
}
