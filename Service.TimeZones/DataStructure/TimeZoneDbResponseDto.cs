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
}