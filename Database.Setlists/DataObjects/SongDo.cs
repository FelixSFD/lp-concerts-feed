using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

[Table("Song")]
public class SongDo : BaseDo, ILinkinpediaLinkable
{
    /// <summary>
    /// Unique ID of the song
    /// </summary>
    [Key]
    public uint Id { get; set; }
    
    /// <summary>
    /// ID of the Album that contains this song
    /// </summary>
    public uint? AlbumId { get; set; }
    
    /// <summary>
    /// Album that contains this song
    /// </summary>
    [ForeignKey(nameof(AlbumId))]
    public virtual AlbumDo? Album { get; set; }

    /// <summary>
    /// Name of the song
    /// </summary>
    [Column("Title")]
    [MaxLength(31)]
    public required string Title { get; set; }

    /// <summary>
    /// ISRC code helps to find the song on Apple Music or Spotify
    /// </summary>
    [Column("Isrc")]
    [MaxLength(15)]
    public string? Isrc { get; set; }
    
    /// <inheritdoc/>
    [Column("LinkinpediaUrl")]
    [MaxLength(63)]
    public string? LinkinpediaUrl { get; set; }
}