using System.Text.Json.Serialization;

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public FileType Type { get; set; }


    /// <summary>
    /// ID of the concerts where this file is linked to
    /// </summary>
    [JsonPropertyName("concertId")]
    public required string ConcertId { get; set; }


    /// <summary>
    /// Type of the file to be uploaded
    /// </summary>
    [JsonPropertyName("contentType")]
    public required string ContentType { get; set; }
}