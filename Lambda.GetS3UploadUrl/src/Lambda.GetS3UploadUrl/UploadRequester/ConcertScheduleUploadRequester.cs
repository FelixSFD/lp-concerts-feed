using Amazon.S3;
using LPCalendar.DataStructure.Requests;

namespace Lambda.GetS3UploadUrl.UploadRequester;

public class ConcertScheduleUploadRequester(GetS3UploadUrlRequest uploadUrlRequest, IGuidService guidService, IAmazonS3 s3Client) : BaseUploadRequester(s3Client)
{
    protected override string GetBucketName()
    {
        return Environment.GetEnvironmentVariable("CONCERT_IMG_BUCKET_NAME") ?? "BUCKET_NOT_DEFINED";
    }

    protected override string GetFileKey()
    {
        string subDir = uploadUrlRequest.Type switch
        {
            GetS3UploadUrlRequest.FileType.ConcertSchedule => "schedule",
            _ => throw new NotImplementedException($"Type '{uploadUrlRequest.Type}' not implemented!")
        };

        return Path.Combine(uploadUrlRequest.ConcertId, subDir, guidService.Random().ToString());
    }

    protected override string GetContentType()
    {
        return uploadUrlRequest.ContentType;
    }
}