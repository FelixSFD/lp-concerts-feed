using System.Text.Json.Serialization;

namespace Lambda.GetTimeZone;

/// <summary>
/// Response object from the TimeZone API
/// (only fields that we use in this project. API might return more than that)
/// </summary>
public class TimeZoneDbResponse
{
    /// <summary>
    /// Name of the Timezone
    /// </summary>
    [JsonPropertyName("zoneName")]
    public required string ZoneName { get; set; }
}