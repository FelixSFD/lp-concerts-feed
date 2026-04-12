using System.Text.Json.Serialization;

namespace Common.WikiMedia.DTOs;

public class LicenseDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}