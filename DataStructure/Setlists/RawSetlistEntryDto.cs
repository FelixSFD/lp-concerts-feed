using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Represents a single entry in a setlist with all its raw data. (so no calculated titles)
/// </summary>
public class RawSetlistEntryDto
{
    /// <summary>
    /// Unique ID of the entry
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// Number of the act.
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
    /// This field can be used to override automatic generated titles
    /// </summary>
    [JsonPropertyName("titleOverride")]
    public string? TitleOverride { get; set; }
    
    /// <summary>
    /// Field to store additional notes about this entry
    /// </summary>
    [JsonPropertyName("extraNotes")]
    public string? ExtraNotes { get; set; }
    
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
}