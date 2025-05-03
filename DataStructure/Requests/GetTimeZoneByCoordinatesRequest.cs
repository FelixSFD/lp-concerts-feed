using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Requests;

public class GetTimeZoneByCoordinatesRequest
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }
    
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}