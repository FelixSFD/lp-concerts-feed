namespace LPCalendar.DataStructure.Requests;

public class GetS3UploadUrlRequest
{
    public enum FileType
    {
        ConcertSchedule,
        LpuInfo
    }


    /// <summary>
    /// Type of the file to upload. (not the MIME-type)
    /// This tells the lambda function for example, which S3 bucket or folder to use
    /// </summary>
    public FileType Type { get; set; }


    /// <summary>
    /// ID of the concerts where this file is linked to
    /// </summary>
    public required string ConcertId { get; set; }


    /// <summary>
    /// Type of the file to be uploaded
    /// </summary>
    public required string ContentType { get; set; }
}