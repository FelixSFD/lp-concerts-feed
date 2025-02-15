using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

public class GetS3UploadUrlResponse
{
    /// <summary>
    /// URL to use when uploading the file. This URL is only valid for the requested file
    /// </summary>
    [JsonPropertyName("uploadUrl")]
    public required string UploadUrl { get; set; }
}