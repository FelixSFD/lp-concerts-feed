using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

/// <summary>
/// Variation of a <see cref="SongDo"/>
/// </summary>
[Table("SongVariants")]
public class SongVariantDo
{
    /// <summary>
    /// Unique ID of the variant
    /// </summary>
    [Key]
    public uint Id { get; set; }
    
    
    /// <summary>
    /// ID of the <see cref="SongDo"/> that is the base for this variation
    /// </summary>
    public uint SongId { get; set; }
    
    /// <summary>
    /// Song that is the base for this variation
    /// </summary>
    [ForeignKey("SongId")]
    public virtual required SongDo Song { get; set; }
    
    
    /// <summary>
    /// Overrides the <see cref="SongDo.Isrc"/> code which helps to find the song on Apple Music or Spotify.
    /// </summary>
    [MaxLength(15)]
    public string? IsrcOverride { get; set; }
    
    
    /// <summary>
    /// Name of this variant. This will be visible as the Song-Title in the setlist
    /// </summary>
    [MaxLength(31)]
    public string? VariantName { get; set; }
    
    
    /// <summary>
    /// Optional description of the variant. What makes this variant different from the original?
    /// </summary>
    [MaxLength(63)]
    public string? Description { get; set; }
}