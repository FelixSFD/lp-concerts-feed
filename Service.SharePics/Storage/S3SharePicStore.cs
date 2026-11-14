using Amazon.S3;
using Microsoft.Extensions.Logging;

namespace Service.SharePics.Storage;

/// <summary>
/// Stores share pics in S3
/// </summary>
public class S3SharePicStore(IAmazonS3 s3Client, S3SharePicStoreOptions options, ILogger<S3SharePicStore> logger) : ISharePicStore
{
    public async Task StorePermanentAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            fileKey = options.KeyPrefix + fileKey;
        }
        
        logger.LogDebug("Storing object '{objectKey}' permanently in bucket '{bucketName}'...", fileKey, options.BucketName);
        var additionalProperties = new Dictionary<string, object>();
        await s3Client.UploadObjectFromStreamAsync(options.BucketName, fileKey, fileStream, additionalProperties,
            cancellationToken);
        
        logger.LogDebug("Successfully stored '{objectKey}' in bucket '{bucketName}'.", fileKey, options.BucketName);
    }
}


public class S3SharePicStoreOptions
{
    public required string BucketName { get; set; }

    public string? KeyPrefix { get; set; }
}
