using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to create a mashup of two or more songs
/// </summary>
public class CreateSongMashupRequestDto
{
    /// <summary>
    /// Name of this mashup
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// IDs of the songs that are included in this mashup
    /// </summary>
    [JsonPropertyName("songIds")]
    public required uint[] SongIds { get; set; }
}