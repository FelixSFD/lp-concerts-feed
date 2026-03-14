using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Information about a setlist without any entries
/// </summary>
public class SetlistHeaderDto
{
    /// <summary>
    /// ID of the setlist
    /// </summary>
    [JsonPropertyName("id")]
    public uint Id { get; set; }
    
    /// <summary>
    /// ID of the <see cref="Concert"/> where this set was played
    /// </summary>
    [JsonPropertyName("concertId")]
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Title of the concert
    /// </summary>
    [JsonPropertyName("concertTitle")]
    public required string ConcertTitle { get; set; }
    
    /// <summary>
    /// Name of the set.
    /// Example: Set A
    /// </summary>
    [JsonPropertyName("setName")]
    public string? SetName { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}