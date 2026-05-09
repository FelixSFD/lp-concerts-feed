using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Represents a single entry in a setlist
/// </summary>
public class SetlistEntryDto
{
    /// <summary>
    /// Unique ID of the entry
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// Number of the act. The full object should not always be returned in order to keep the JSON smaller
    /// </summary>
    [JsonPropertyName("actNumber")]
    public uint? ActNumber { get; set; }
    
    /// <summary>
    /// Number to sort the entries
    /// </summary>
    [JsonPropertyName("sortNumber")]
    public uint SortNumber { get; set; }
    
    /// <summary>
    /// Number of the song in this setlist. Is displayed as an orientation
    /// </summary>
    [JsonPropertyName("songNumber")]
    public ushort SongNumber { get; set; }

    /// <summary>
    /// the song variant that has been played
    /// </summary>
    [JsonPropertyName("playedSongVariant")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SongVariantDto? PlayedSongVariant { get; set; }
    
    /// <summary>
    /// the song that has been played
    /// </summary>
    [JsonPropertyName("playedSong")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SongDto? PlayedSong { get; set; }
    
    /// <summary>
    /// the song mashup that has been played
    /// </summary>
    [JsonPropertyName("playedSongMashup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SongMashupDto? PlayedSongMashup { get; set; }
    
    /// <summary>
    /// Title of this setlist entry. This can for example be the title of a song or a mashup
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// Field to store additional notes about this entry
    /// </summary>
    [JsonPropertyName("extraNotes")]
    public string? ExtraNotes { get; set; }
    
    /// <summary>
    /// ISRC for this entry. This can be used to find the song on Apple Music or Spotify
    /// </summary>
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; }
    
    /// <summary>
    /// Unique ID for the Song on Apple Music
    /// </summary>
    [JsonPropertyName("appleMusicId")]
    public string? AppleMusicId { get; set; }
    
    /// <summary>
    /// URL to a page in Linkinpedia about the song/variant/mashup
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// true if this slot is known to rotate between shows on the same tour.
    /// </summary>
    [JsonPropertyName("isRotationSong")]
    public bool IsRotationSong { get; set; }

    /// <summary>
    /// true if the song was played from a recording only. Not played live. (this can happen for pre-show-songs for example)
    /// </summary>
    [JsonPropertyName("isPlayedFromRecording")]
    public bool IsPlayedFromRecording { get; set; }
    
    /// <summary>
    /// true if this song has not only not been played before, but was not released before this show.
    /// Example: "Heavy Is The Crown" in Hamburg 2024
    /// </summary>
    [JsonPropertyName("isWorldPremiere")]
    public bool IsWorldPremiere { get; set; }
    
    /// <summary>
    /// true if this song was already released, but played live for the first time.
    /// Example: "Keys To The Kingdom" in Los Angeles 2024
    /// </summary>
    [JsonPropertyName("isLivePremiere")]
    public bool IsLivePremiere { get; set; }

    /// <summary>
    /// List of extras included in this song. This can for example be an extended bridge with parts of another song
    /// </summary>
    [JsonPropertyName("songExtras")]
    public List<SetlistEntrySongExtraDto> SongExtras { get; set; } = [];
}