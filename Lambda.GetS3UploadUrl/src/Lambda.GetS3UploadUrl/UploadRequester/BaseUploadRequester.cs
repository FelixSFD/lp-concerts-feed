using Amazon.S3;
using Amazon.S3.Model;
using LPCalendar.DataStructure.Requests;

namespace Lambda.GetS3UploadUrl.UploadRequester;

public abstract class BaseUploadRequester(IAmazonS3 s3Client)
{
    /// <summary>
    /// Name of the bucket to use
    /// </summary>
    /// <returns></returns>
    protected abstract string GetBucketName();

    /// <summary>
    /// Key of the file in S3
    /// </summary>
    /// <returns></returns>
    public abstract string GetFileKey();


    /// <summary>
    /// Content-type of the file to be uploaded
    /// </summary>
    /// <returns></returns>
    protected abstract string GetContentType();


    /// <summary>
    /// Generate URL to upload the file to
    /// </summary>
    /// <returns></returns>
    public string GetUploadUrl() 
        => GetUploadUrl(TimeSpan.FromMinutes(15));
    
    
    /// <summary>
    /// Generate URL to upload the file to
    /// </summary>
    /// <param name="validity">How long the link will be valid</param>
    /// <returns></returns>
    public string GetUploadUrl(TimeSpan validity)
    {
        return s3Client.GetPreSignedURL(new GetPreSignedUrlRequest()
        {
            BucketName = GetBucketName(),
            Key = GetFileKey(),
            ContentType = GetContentType(),
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(validity)
        });
    }
}