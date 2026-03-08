using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

[Table("SetlistEntry")]
public class SetlistEntryDo : BaseDo
{
    /// <summary>
    /// Unique ID of the entry (GUID?)
    /// </summary>
    [Key]
    public required string Id { get; set; }

    /// <summary>
    /// ID of the setlist
    /// </summary>
    public uint SetlistId { get; set; }
    
    /// <summary>
    /// Setlist that contains this entry
    /// </summary>
    public virtual SetlistDo Setlist { get; set; }
    
    /// <summary>
    /// Number of the <see cref="SetlistActDo"/>
    /// </summary>
    public uint? ActNumber { get; set; }
    
    /// <summary>
    /// Act of this entry. Is used to group multiple songs in a setlist
    /// </summary>
    public virtual SetlistActDo? Act { get; set; }
    
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
    /// ID of the <see cref="SongDo"/> that was played.
    /// </summary>
    /// <remarks>Only one of <see cref="PlayedSongId"/>, <see cref="PlayedSongVariantId"/> and <see cref="PlayedSongId"/> can be set at the same time</remarks>
    public uint? PlayedSongId { get; set; }
    
    /// <summary>
    /// Song that was played
    /// </summary>
    [ForeignKey(nameof(PlayedSongId))]
    public virtual SongDo? PlayedSong { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongVariantDo"/> that was played.
    /// </summary>
    public uint? PlayedSongVariantId { get; set; }
    
    /// <summary>
    /// Song variant that was played
    /// </summary>
    [ForeignKey(nameof(PlayedSongVariantId))]
    public virtual SongVariantDo? PlayedSongVariant { get; set; }
    
    /// <summary>
    /// ID of the <see cref="SongVariantDo"/> that was played.
    /// </summary>
    public uint? PlayedMashupId { get; set; }
    
    /// <summary>
    /// Mashup that was played
    /// </summary>
    [ForeignKey(nameof(PlayedMashupId))]
    public virtual SongMashupDo? PlayedMashup { get; set; }
    
    /// <summary>
    /// Special things that have been added to this played song. This can for example be an extended bridge with the verse of a different song.
    /// </summary>
    public virtual ICollection<SetlistEntrySongExtraDo> SongExtras { get; set; }
    
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