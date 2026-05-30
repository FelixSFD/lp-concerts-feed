using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

[Table("Tour")]
public class TourDo
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    [Column("Id")]
    [MaxLength(DataConstants.TourIdLength)]
    public required string Id { get; set; }

    /// <summary>
    /// Name of this tour
    /// </summary>
    [Column("Name")]
    [MaxLength(DataConstants.TitleFieldLength)]
    public required string Name { get; set; }
    
    /// <summary>
    /// All legs of this tour
    /// </summary>
    public ICollection<TourLegDo> Legs { get; set; } = [];
}