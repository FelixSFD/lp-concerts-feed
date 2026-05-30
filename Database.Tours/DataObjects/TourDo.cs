using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Tours.DataObjects;

[Table("Tour")]
public class TourDo
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    [Column("Id")]
    public uint Id { get; set; }

    /// <summary>
    /// Name of this tour
    /// </summary>
    [Column("Name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// All legs of this tour
    /// </summary>
    public ICollection<TourLegDo> Legs { get; set; } = [];
}