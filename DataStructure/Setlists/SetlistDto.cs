using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Information about a setlist
/// </summary>
public class SetlistDto
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
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }

    /// <summary>
    /// Entries of the setlist
    /// </summary>
    [JsonPropertyName("entries")]
    public List<SetlistEntryDto> Entries { get; set; } = [];
}