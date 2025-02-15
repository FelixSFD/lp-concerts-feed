using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using LPCalendar.DataStructure;

namespace Lambda.GetS3UploadUrl.UploadRequester;

public abstract class BaseConcertUploadRequester(IAmazonS3 s3Client) : BaseUploadRequester(s3Client)
{
    public abstract Task UpdateConcert(string concertId, IDynamoDBContext dbContext, DBOperationConfigProvider dbOperationConfigProvider);
}