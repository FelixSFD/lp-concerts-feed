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
    /// ID of the <see cref="Song"/> that was played.
    /// </summary>
    /// <remarks>Only one of <see cref="PlayedSongId"/>, <see cref="PlayedSongVariantId"/> and <see cref="PlayedSongId"/> can be set at the same time</remarks>
    public uint? PlayedSongId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongVariant"/> that was played.
    /// </summary>
    public uint? PlayedSongVariantId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongVariant"/> that was played.
    /// </summary>
    public uint? PlayedMashupId { get; set; }
}