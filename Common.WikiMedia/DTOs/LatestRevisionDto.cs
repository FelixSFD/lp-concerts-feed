using System.Text.Json.Serialization;

namespace Common.WikiMedia.DTOs;

public class LatestRevisionDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}