using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Requests;

namespace Lambda.GetS3UploadUrl.UploadRequester;

public class ConcertScheduleUploadRequester(GetS3UploadUrlRequest uploadUrlRequest, IGuidService guidService, IAmazonS3 s3Client) 
    : BaseConcertUploadRequester(s3Client)
{
    private readonly Lazy<string> _fileKey = new(() =>
        Path.Combine(uploadUrlRequest.ConcertId, "schedule", guidService.Random().ToString()));
    
    protected override string GetBucketName()
    {
        return Environment.GetEnvironmentVariable("CONCERT_IMG_BUCKET_NAME") ?? "BUCKET_NOT_DEFINED";
    }

    public override string GetFileKey()
    {
        return _fileKey.Value;
    }

    protected override string GetContentType()
    {
        return uploadUrlRequest.ContentType;
    }


    public override async Task UpdateConcert(string concertId, IDynamoDBContext dbContext, DBOperationConfigProvider dbOperationConfigProvider)
    {
        var concert = await dbContext
            .LoadAsync<Concert>(concertId, dbOperationConfigProvider.GetConcertsConfigWithEnvTableName());

        concert.ScheduleImageFile = GetFileKey();

        await dbContext.SaveAsync(concert);
    }
}