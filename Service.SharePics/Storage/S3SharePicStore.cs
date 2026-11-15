using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace Service.SharePics.Storage;

/// <summary>
/// Stores share pics in S3
/// </summary>
public class S3SharePicStore(IAmazonS3 s3Client, S3SharePicStoreOptions options, ILogger<S3SharePicStore> logger) : ISharePicStore
{
    public async Task StorePermanentAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken = default)
    {
        Log.StoringObjectPermanently(logger, fileKey, options.BucketName);
        await StoreObjectAsync(fileStream, fileKey, cancellationToken);
        Log.StoredObjectPermanently(logger, fileKey, options.BucketName);
    }

    public async Task StoreTemporaryAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken = default)
    {
        Log.StoringObjectTemporarily(logger, fileKey, options.BucketName);
        var tags = new List<Tag>
        {
            new()
            {
                Key = "temporary",
                Value = "true"
            }
        };
        await StoreObjectAsync(fileStream, fileKey, tags, cancellationToken);
        Log.StoredObjectTemporarily(logger, fileKey, options.BucketName);
    }

    private async Task StoreObjectAsync(Stream fileStream, string fileKey, CancellationToken cancellationToken)
    {
        var tags = new List<Tag>();
        await StoreObjectAsync(fileStream, fileKey, tags, cancellationToken);
    }
    
    private async Task StoreObjectAsync(Stream fileStream, string fileKey, List<Tag> tags,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            fileKey = options.KeyPrefix + fileKey;
            Log.AddedPrefixToObjectKey(logger, fileKey);
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = fileKey,
            TagSet = tags,
            InputStream = fileStream,
        };
        await s3Client.PutObjectAsync(putRequest, cancellationToken);
    }
}

/// <summary>
/// Options to configure the <see cref="S3SharePicStore"/>
/// </summary>
public class S3SharePicStoreOptions
{
    /// <summary>
    /// Name of the S3 bucket
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Prefix to add to every file
    /// </summary>
    public string? KeyPrefix { get; set; }
}
