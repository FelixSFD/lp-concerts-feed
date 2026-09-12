using System.Text.Json.Serialization;

namespace Service.Tours.DataStructure;

/// <summary>
/// Basic information about a concert in Linkinpedia
/// </summary>
public class LinkinpediaConcertListEntryBo
{
    /// <summary>
    /// ID of the wiki page
    /// </summary>
    [JsonPropertyName("ShowPage")]
    public required string WikiPageId { get; set; }

    /// <summary>
    /// Date of the concert as string
    /// </summary>
    [JsonPropertyName("Date")]
    public required string DateString { get; set; }

    /// <summary>
    /// type of show
    /// </summary>
    [JsonPropertyName("ShowType")]
    public required string ShowType { get; set; }

    [JsonPropertyName("Country")]
    public required string Country { get; set; }
    
    [JsonPropertyName("State")]
    public required string State { get; set; }
    
    [JsonPropertyName("Province")]
    public required string Province { get; set; }
    
    [JsonPropertyName("UkCountry")]
    public required string UkCountry { get; set; }
    
    [JsonPropertyName("City")]
    public required string City { get; set; }
    
    [JsonPropertyName("Venue")]
    public required string Venue { get; set; }
    
    [JsonPropertyName("Tour")]
    public required string Tour { get; set; }
    
    [JsonPropertyName("TourLeg")]
    public required string TourLeg { get; set; }
}