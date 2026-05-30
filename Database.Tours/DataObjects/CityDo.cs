using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.DataObjects;

[Table("City")]
[PrimaryKey(nameof(CountryCode), nameof(StateCode), nameof(Id))]
public class CityDo : BaseDo
{
    /// <summary>
    /// Country code. This is part of the composite key
    /// </summary>
    [Column("CountryCode")]
    [MaxLength(DataConstants.CountryCodeLength)]
    public required string CountryCode { get; set; }
    
    /// <summary>
    /// State code. This is part of the composite key
    /// </summary>
    [Column("StateCode")]
    [MaxLength(DataConstants.StateCodeLength)]
    public string? StateCode { get; set; }
    
    /// <summary>
    /// City-ID. This is part of the composite key
    /// </summary>
    [Column("Id")]
    public uint Id { get; set; }
    
    /// <summary>
    /// English name of this city
    /// </summary>
    [Column("Name")]
    [MaxLength(DataConstants.CityNameLength)]
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this city in its native language
    /// </summary>
    [Column("NativeName")]
    [MaxLength(DataConstants.CityNameLength)]
    public required string NativeName { get; set; }
    
    /// <summary>
    /// Country where this city is located in
    /// </summary>
    [ForeignKey(nameof(CountryCode))]
    public virtual CountryDo Country { get; set; }
    
    /// <summary>
    /// State where this state is located in (if applicable)
    /// </summary>
    public virtual StateDo? State { get; set; }
}