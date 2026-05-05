using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

/// <summary>
/// Variation of a <see cref="SongDo"/>
/// </summary>
[Table("SongVariant")]
public class SongVariantDo : BaseDo
{
    /// <summary>
    /// Unique ID of the variant
    /// </summary>
    [Key]
    [Column("Id")]
    public uint Id { get; set; }
    
    
    /// <summary>
    /// ID of the <see cref="SongDo"/> that is the base for this variation
    /// </summary>
    [Column("SongId")]
    public uint SongId { get; set; }
    
    /// <summary>
    /// Song that is the base for this variation
    /// </summary>
    [ForeignKey("SongId")]
    public virtual SongDo Song { get; set; }
    
    
    /// <summary>
    /// Overrides the <see cref="SongDo.Isrc"/> code which helps to find the song on Apple Music or Spotify.
    /// </summary>
    [MaxLength(DataConstants.IsrcLength)]
    [Column("IsrcOverride")]
    public string? IsrcOverride { get; set; }
    
    
    /// <summary>
    /// Overrides the <see cref="SongDo.AppleMusicId"/> which is a unique ID for the Song on Apple Music
    /// </summary>
    [Column("appleMusicIdOverride")]
    [MaxLength(DataConstants.AppleMusicIdLength)]
    public string? AppleMusicIdOverride { get; set; }
    
    
    /// <summary>
    /// Name of this variant. This will be visible as the Song-Title in the setlist
    /// </summary>
    [MaxLength(DataConstants.TitleFieldLength)]
    [Column("VariantName")]
    public string? VariantName { get; set; }
    
    
    /// <summary>
    /// Optional description of the variant. What makes this variant different from the original?
    /// </summary>
    [MaxLength(127)]
    [Column("Description")]
    public string? Description { get; set; }
}