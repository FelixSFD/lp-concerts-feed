using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to update the information of a <see cref="SongMashupDto"/>
/// </summary>
public class UpdateSongMashupRequestDto
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