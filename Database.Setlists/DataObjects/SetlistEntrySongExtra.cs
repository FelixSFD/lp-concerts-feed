namespace Database.Setlists.DataObjects;

/// <summary>
/// This table is used when the band added a small part like a verse to another song without doing a full mashup.
/// And example for this is playing the "Cut The Bridge" bridge in the extended bridge of "Bleed It Out".
/// </summary>
public class SetlistEntrySongExtra
{
    public enum ExtraType
    {
        ExtendedBridge,
        ExtraVerse
    }
    
    /// <summary>
    /// ID of this extra
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SetlistEntryDo"/> where this extra was included.
    /// </summary>
    public required string SetlistEntryId { get; set; }

    /// <summary>
    /// Optional ID of the <see cref="SongDo"/> that was added to a different song if it's a song that is referenced in our database.
    /// </summary>
    public uint? SongId { get; set; }

    /// <summary>
    /// Defines how the <see cref="SongId"/> was included
    /// </summary>
    public ExtraType Type { get; set; }
    
    /// <summary>
    /// Description of the extra
    /// </summary>
    public required string Description { get; set; }
}