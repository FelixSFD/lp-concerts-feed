using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Represents a single entry in a setlist
/// </summary>
public class SetlistEntryDto
{
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