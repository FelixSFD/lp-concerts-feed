using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Import;

/// <summary>
/// Request to import a setlist
/// </summary>
public class ImportSetlistRequestDto
{
    /// <summary>
    /// ID of the concert
    /// </summary>
    [JsonPropertyName("concertId")]
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Link to the concert page on Linkinpedia. This page must contain the setlist
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public required string LinkinpediaUrl { get; set; }
}