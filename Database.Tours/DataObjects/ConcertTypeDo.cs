using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

[Table("ConcertType")]
public class ConcertTypeDo
{
    [Key]
    [Column("Id")]
    public uint Id { get; set; }

    [Column("Name")]
    [MaxLength(DataConstants.ConcertTypeNameLength)]
    public required string Name { get; set; }
}