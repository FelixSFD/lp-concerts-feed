using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to create a new song
/// </summary>
public class CreateSongRequestDto
{
    /// <summary>
    /// Name of this song
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// ISRC of the song
    /// </summary>
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}