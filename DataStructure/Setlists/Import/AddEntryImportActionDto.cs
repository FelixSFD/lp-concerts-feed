namespace LPCalendar.DataStructure.Setlists.Import;

/// <summary>
/// Use existing data to add an entry to this setlist
/// </summary>
public class AddEntryImportActionDto() : ImportActionDto(ImportActionType.AddSong)
{
    /// <summary>
    /// Number of the act where this entry is included
    /// </summary>
    public uint? ActNumber { get; set; }
    
    /// <summary>
    /// ID of the song
    /// </summary>
    public uint? SongId { get; set; }
    
    /// <summary>
    /// ID of the song variant
    /// </summary>
    public uint? SongVariantId { get; set; }
    
    /// <summary>
    /// ID of the mashup
    /// </summary>
    public uint? MashupId { get; set; }
}