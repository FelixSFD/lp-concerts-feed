using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

/// <summary>
/// A state within a <see cref="CountryDo"/>
/// </summary>
[Table("State")]
public class StateDo : BaseDo
{
    [Key]
    [Column("CountryCode")]
    [MaxLength(DataConstants.CountryCodeLength)]
    public required string CountryCode { get; set; }
    
    [Key]
    [Column("Code")]
    [MaxLength(DataConstants.StateCodeLength)]
    public required string Code { get; set; }
    
    /// <summary>
    /// Country where this state is located in
    /// </summary>
    [ForeignKey(nameof(CountryCode))]
    public virtual CountryDo Country { get; set; }

    /// <summary>
    /// English name of this state
    /// </summary>
    [Column("Name")]
    [MaxLength(DataConstants.CountryNameLength)]
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this state in its native language
    /// </summary>
    [Column("NativeName")]
    [MaxLength(DataConstants.CountryNameLength)]
    public required string NativeName { get; set; }
}