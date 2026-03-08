using System.ComponentModel.DataAnnotations;

namespace LPCalendar.DataStructure.Setlists.Parameters;

/// <summary>
/// Common parameters for setlist entries
/// </summary>
public class SetlistEntryParametersDto
{
    /// <summary>
    /// Number to sort the entries
    /// </summary>
    public uint SortNumber { get; set; }
    
    /// <summary>
    /// Number of the song in this setlist. Is displayed as an orientation
    /// </summary>
    public ushort SongNumber { get; set; }
    
    /// <summary>
    /// Optional property to override the title of the song or mashup in this entry only.
    /// </summary>
    [MaxLength(31)]
    public string? TitleOverride { get; set; }
    
    /// <summary>
    /// Field to store additional notes about this entry
    /// </summary>
    [MaxLength(127)]
    public string? ExtraNotes { get; set; }
    
    /// <summary>
    /// true if this slot is known to rotate between shows on the same tour.
    /// </summary>
    public bool IsRotationSong { get; set; }

    /// <summary>
    /// true if the song was played from a recording only. Not played live. (this can happen for pre-show-songs for example)
    /// </summary>
    public bool IsPlayedFromRecording { get; set; }
    
    /// <summary>
    /// true if this song has not only not been played before, but was not released before this show.
    /// Example: "Heavy Is The Crown" in Hamburg 2024
    /// </summary>
    public bool IsWorldPremiere { get; set; }
}