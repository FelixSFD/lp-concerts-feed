using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

/// <summary>
/// This table is used when the band added a small part like a verse to another song without doing a full mashup.
/// And example for this is playing the "Cut The Bridge" bridge in the extended bridge of "Bleed It Out".
/// </summary>
[Table("SetlistEntrySongExtra")]
public class SetlistEntrySongExtraDo : BaseDo
{
    public enum ExtraType
    {
        ExtendedBridge = 1,
        ExtraVerse = 2
    }
    
    /// <summary>
    /// ID of this extra
    /// </summary>
    [Key]
    [Column("Id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SetlistEntryDo"/> where this extra was included.
    /// </summary>
    [Column("SetlistEntryId")]
    public string SetlistEntryId { get; set; }
    
    /// <summary>
    /// The <see cref="SetlistEntryDo"/> where this extra was included.
    /// </summary>
    [ForeignKey(nameof(SetlistEntryId))]
    public SetlistEntryDo SetlistEntry { get; set; }

    /// <summary>
    /// Optional ID of the <see cref="SongDo"/> that was added to a different song if it's a song that is referenced in our database.
    /// </summary>
    [Column("SongId")]
    public uint? SongId { get; set; }
    
    /// <summary>
    /// <see cref="SongDo"/> that was added to a different song if it's a song that is referenced in our database.
    /// </summary>
    [ForeignKey(nameof(SongId))]
    public SongDo? Song { get; set; }

    /// <summary>
    /// Defines how the <see cref="SongId"/> was included
    /// </summary>
    public ExtraType Type { get; set; }
    
    /// <summary>
    /// Description of the extra
    /// </summary>
    [Column("Description")]
    [MaxLength(127)]
    public required string Description { get; set; }
}