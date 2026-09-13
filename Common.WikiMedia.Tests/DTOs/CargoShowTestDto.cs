using System.Text.Json.Serialization;

namespace Common.WikiMedia.Tests.DTOs;

public class CargoShowTestDto
{
    [JsonPropertyName("Artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("ShowPage")]
    public string? ShowPage { get; set; }

    [JsonPropertyName("Date")]
    public string? Date { get; set; }

    [JsonPropertyName("ShowType")]
    public string? ShowType { get; set; }

    [JsonPropertyName("Country")]
    public string? Country { get; set; }

    [JsonPropertyName("State")]
    public string? State { get; set; }

    [JsonPropertyName("Province")]
    public string? Province { get; set; }

    [JsonPropertyName("UKCountry")]
    public string? UKCountry { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("Venue")]
    public string? Venue { get; set; }

    [JsonPropertyName("Tour")]
    public string? Tour { get; set; }

    [JsonPropertyName("TourLeg")]
    public string? TourLeg { get; set; }

    [JsonPropertyName("Date__precision")]
    public string? DatePrecision { get; set; }
}
