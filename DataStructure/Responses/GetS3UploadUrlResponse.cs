namespace LPCalendar.DataStructure.Responses;

public class GetS3UploadUrlResponse
{
    /// <summary>
    /// URL to use when uploading the file. This URL is only valid for the requested file
    /// </summary>
    public string UploadUrl { get; set; }
}