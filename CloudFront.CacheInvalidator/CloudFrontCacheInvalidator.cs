using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;

namespace CloudFront.CacheInvalidator;

public static class CloudFrontCacheInvalidator
{
    /// <summary>
    /// Invalidates the cache of given paths in CloudFront
    /// </summary>
    /// <param name="distributionId">ID of your CF distribution</param>
    /// <param name="pathsToInvalidate"></param>
    /// <returns></returns>
    public static async Task<CreateInvalidationResponse> InvalidateAsync(string distributionId, params string[] pathsToInvalidate)
    {
        // Create a CloudFront client
        var cloudFrontClient = new AmazonCloudFrontClient(RegionEndpoint.USEast1); // Choose appropriate region
        // Create invalidation request
        var invalidationRequest = new CreateInvalidationRequest
        {
            DistributionId = distributionId,
            InvalidationBatch = new InvalidationBatch
            {
                CallerReference = DateTime.UtcNow.Ticks.ToString(), // Unique value for each invalidation
                Paths = new Paths
                {
                    Quantity = pathsToInvalidate.Length,
                    Items = pathsToInvalidate.ToList()
                }
            }
        };

        // Send the invalidation request
        return await cloudFrontClient.CreateInvalidationAsync(invalidationRequest);
    }
}