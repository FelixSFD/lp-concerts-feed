using Microsoft.Extensions.Configuration;

namespace Service.Tours;

internal static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// Returns the name of the S3 bucket where concert images (like schedules) are stored
        /// </summary>
        /// <returns></returns>
        public string? GetConcertImageBucketName()
        {
            return configuration.GetValue<string>("ConcertImageUpload:BucketName");
        }
    }
}