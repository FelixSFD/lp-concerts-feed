using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

[Table("Album")]
public class AlbumDo : BaseDo, ILinkinpediaLinkable
{
    /// <summary>
    /// Unique ID of the album
    /// </summary>
    [Key]
    [Column("Id")]
    public uint Id { get; set; }

    /// <summary>
    /// Name of the album
    /// </summary>
    [Column("Title")]
    [MaxLength(DataConstants.AlbumTitleFieldLength)]
    public required string Title { get; set; }

    /// <inheritdoc/>
    [Column("LinkinpediaUrl")]
    [MaxLength(DataConstants.LinkinpediaUrlLength)]
    public string? LinkinpediaUrl { get; set; }
}