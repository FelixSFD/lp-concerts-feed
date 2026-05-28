using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

[Table("City")]
public class CityDo : BaseDo
{
    [Key]
    [Column("CountryCode")]
    [MaxLength(DataConstants.CountryCodeLength)]
    public required string CountryCode { get; set; }
    
    [Key]
    [Column("StateCode")]
    [MaxLength(DataConstants.StateCodeLength)]
    public string? StateCode { get; set; }
    
    [Key]
    [Column("Id")]
    public uint Id { get; set; }
    
    /// <summary>
    /// Country where this city is located in
    /// </summary>
    [ForeignKey(nameof(CountryCode))]
    public virtual CountryDo Country { get; set; }
    
    /// <summary>
    /// State where this state is located in (if applicable)
    /// </summary>
    [ForeignKey(nameof(CountryCode))]
    public virtual StateDo? State { get; set; }
}