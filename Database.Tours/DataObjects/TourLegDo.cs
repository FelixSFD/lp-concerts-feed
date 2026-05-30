using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

/// <summary>
/// One leg of a <see cref="TourDo"/>. This can for example be the European Part of a World Tour
/// </summary>
[Table("TourLeg")]
public class TourLegDo
{
    /// <summary>
    /// ID of the <see cref="TourDo"/>
    /// </summary>
    [Key]
    [Column("TourId")]
    [MaxLength(DataConstants.TourIdLength)]
    public required string TourId { get; set; }
    
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    [Column("Id")]
    [MaxLength(DataConstants.TourIdLength)]
    public required string Id { get; set; }

    /// <summary>
    /// Name of this tour leg
    /// </summary>
    [Column("Name")]
    [MaxLength(DataConstants.TitleFieldLength)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Tour that this leg is a part of
    /// </summary>
    [ForeignKey(nameof(TourId))]
    public virtual TourDo Tour { get; set; }
}