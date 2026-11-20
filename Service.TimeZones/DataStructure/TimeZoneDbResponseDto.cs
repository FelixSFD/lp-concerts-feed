using System.Text.Json.Serialization;

namespace Service.TimeZones.DataStructure;

/// <summary>
/// Response object from the TimeZone API
/// (only fields that we use in this project. API might return more than that)
/// </summary>
internal class TimeZoneDbResponseDto
{
    /// <summary>
    /// Name of the Timezone
    /// </summary>
    [JsonPropertyName("zoneName")]
    public required string ZoneName { get; set; }
    
    /// <summary>
    /// Status of the request. The API is stupid and returns errors with HTTP 200, so we have to check this field to see if the request was successful or not.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }

    /// <summary>
    /// Error message if the request was not successful. If the request was successful, this field will be null.
    /// </summary>
    [JsonPropertyName("message")]
    public required string? Message { get; set; }
}