using Amazon.S3;
using Amazon.S3.Model;
using Database.Tours.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

namespace Service.Tours;

/// <summary>
/// Service to manage uploads for concert-related images like the schedule (for modern concerts)
/// </summary>
public interface IConcertImageUploadService
{
    public Task<string> GetPresignedScheduleUploadUrlAsync(string concertId, string contentType);
}

/// <summary>
/// Service to manage uploads for concert-related images like the schedule (for modern concerts)
/// </summary>
internal class ConcertImageUploadService(IAmazonS3 s3Client, IConcertRepository concertRepository, IConfiguration appSettings, ILogger<ConcertImageUploadService> logger) : IConcertImageUploadService
{
    public async Task<string> GetPresignedScheduleUploadUrlAsync(string concertId, string contentType)
    {
        logger.LogDebug("Getting presigned URL for file upload for concert: {concertId}", concertId);
        var concert = await concertRepository.GetByPrimaryKeyAsync(concertId) ?? throw new ConcertNotFoundException(concertId);
        var getPresignedUrlRequest = new GetPreSignedUrlRequest
        {
            BucketName = appSettings.GetConcertImageBucketName(),
            Key = $"{concertId}/schedule/{Guid.NewGuid().ToString().ToLower()}",
            ContentType = contentType,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
        };
        var uploadUrl = await s3Client.GetPreSignedURLAsync(getPresignedUrlRequest);
        
        concert.ScheduleImageFile = getPresignedUrlRequest.Key;
        concertRepository.Update(concert);
        await concertRepository.SaveChangesAsync();
        
        logger.LogDebug("Generated presigned URL: {uploadUrl}", uploadUrl);
        return uploadUrl;
    }
}

public static class ConcertImageUploadServiceCollectionExtensions
{
    extension(IServiceCollection serviceProvider)
    {
        /// <summary>
        /// Registers the <see cref="IConcertImageUploadService"/> and its dependencies
        /// </summary>
        public void AddConcertImageUploadService()
        {
            serviceProvider.AddAWSService<IAmazonS3>();
            serviceProvider.AddScoped<IConcertImageUploadService, ConcertImageUploadService>();
        }
    }
}