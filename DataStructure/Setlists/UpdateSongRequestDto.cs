using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to update the information of a <see cref="SongDto"/>
/// </summary>
public class UpdateSongRequestDto
{
    /// <summary>
    /// Name of this song
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// ID of the album
    /// </summary>
    [JsonPropertyName("albumId")]
    public uint? AlbumId { get; set; }
    
    /// <summary>
    /// ISRC of the song
    /// </summary>
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; }
    
    /// <summary>
    /// Unique ID for the Song on Apple Music
    /// </summary>
    [JsonPropertyName("appleMusicId")]
    public string? AppleMusicId { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}