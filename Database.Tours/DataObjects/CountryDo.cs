using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

[Table("Country")]
public class CountryDo : BaseDo
{
    /// <summary>
    /// The 3-letter ISO code for this country
    /// </summary>
    [Key]
    [Column("IsoCode")]
    [MaxLength(DataConstants.CountryCodeLength)]
    [MinLength(DataConstants.CountryCodeLength)]
    public required string IsoCode { get; set; }
    
    /// <summary>
    /// English name of this country
    /// </summary>
    [Column("Name")]
    [MaxLength(DataConstants.CountryNameLength)]
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this country in its native language
    /// </summary>
    [Column("NativeName")]
    [MaxLength(DataConstants.CountryNameLength)]
    public required string NativeName { get; set; }
}