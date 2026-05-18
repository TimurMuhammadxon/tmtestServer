using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OnlineTesting.Infrastructure.Storage;

public class BucketInitializer : IHostedService
{
    private readonly IAmazonS3 _s3;
    private readonly StorageOptions _options;
    private readonly ILogger<BucketInitializer> _logger;

    public BucketInitializer(IAmazonS3 s3, IOptions<StorageOptions> options, ILogger<BucketInitializer> logger)
    {
        _s3 = s3;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            await _s3.GetBucketLocationAsync(_options.BucketName, ct);
            _logger.LogInformation("Storage bucket '{Bucket}' already exists.", _options.BucketName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _s3.PutBucketAsync(new PutBucketRequest { BucketName = _options.BucketName }, ct);
            _logger.LogInformation("Storage bucket '{Bucket}' created.", _options.BucketName);
        }

        await SetPublicReadPolicyAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private async Task SetPublicReadPolicyAsync(CancellationToken ct)
    {
        var policy = $$"""
            {
              "Version": "2012-10-17",
              "Statement": [{
                "Effect": "Allow",
                "Principal": {"AWS": ["*"]},
                "Action": ["s3:GetObject"],
                "Resource": ["arn:aws:s3:::{{_options.BucketName}}/*"]
              }]
            }
            """;

        await _s3.PutBucketPolicyAsync(new PutBucketPolicyRequest
        {
            BucketName = _options.BucketName,
            Policy = policy
        }, ct);
    }
}
