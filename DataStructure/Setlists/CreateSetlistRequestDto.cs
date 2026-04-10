using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Creates a new setlist for a concert
/// </summary>
public class CreateSetlistRequestDto
{
    /// <summary>
    /// ID of the <see cref="Concert"/> where this set was played
    /// </summary>
    [JsonPropertyName("concertId")]
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Title of the concert
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("concertTitle")]
    public required string ConcertTitle { get; set; }
    
    /// <summary>
    /// Name of the set.
    /// Example: Set A
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("setName")]
    public string? SetName { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// If set, the songs are appended automatically to this setlist
    /// </summary>
    [JsonPropertyName("addSongs")]
    public List<AddSongToSetlistRequestDto>? SongEntries { get; set; }
    
    /// <summary>
    /// If set, the song variants are appended automatically to this setlist
    /// </summary>
    [JsonPropertyName("addSongVariants")]
    public List<AddSongVariantToSetlistRequestDto>? SongVariantEntries { get; set; }
    
    /// <summary>
    /// If set, the mashups are appended automatically to this setlist
    /// </summary>
    [JsonPropertyName("addSongMashups")]
    public List<AddSongMashupToSetlistRequestDto>? MashupEntries { get; set; }
}