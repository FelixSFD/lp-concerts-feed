using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

public class GetTimeZoneByCoordinatesResponse
{
    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId { get; set; }
}