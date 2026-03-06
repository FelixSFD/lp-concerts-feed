namespace Database.Setlists.DataObjects;

public class SetlistEntry
{
    /// <summary>
    /// Unique ID of the entry (GUID?)
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// ID of the setlist
    /// </summary>
    public uint SetlistId { get; set; }
    
    /// <summary>
    /// Number of the <see cref="SetlistAct"/>
    /// </summary>
    public uint ActNumber { get; set; }
    
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
    public string? TitleOverride { get; set; }
    
    /// <summary>
    /// Field to store additional notes about this entry
    /// </summary>
    public string? ExtraNotes { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongDo"/> that was played.
    /// </summary>
    /// <remarks>Only one of <see cref="PlayedSongId"/>, <see cref="PlayedSongVariantId"/> and <see cref="PlayedSongId"/> can be set at the same time</remarks>
    public uint? PlayedSongId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongVariantDo"/> that was played.
    /// </summary>
    public uint? PlayedSongVariantId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongVariantDo"/> that was played.
    /// </summary>
    public uint? PlayedMashupId { get; set; }
    
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