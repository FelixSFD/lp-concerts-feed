using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Information about a song
/// </summary>
public class SongDto
{
    /// <summary>
    /// ID of the song
    /// </summary>
    [JsonPropertyName("id")]
    public uint Id { get; set; }
    
    /// <summary>
    /// Title of the song
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    /// <summary>
    /// Album that contains this song
    /// </summary>
    [JsonPropertyName("album")]
    public AlbumDto? Album { get; set; }
    
    /// <summary>
    /// ISRC of the song
    /// </summary>
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; }
    
    /// <summary>
    /// Link to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}