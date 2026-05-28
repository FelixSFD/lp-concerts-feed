using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

/// <summary>
/// A venue where a concert was played
/// </summary>
[Table("Venue")]
public class VenueDo : BaseDo
{
    /// <summary>
    /// Unique ID of this venue
    /// </summary>
    [Key]
    public uint Id { get; set; }
    
    [Column("CountryCode")]
    [MaxLength(DataConstants.CountryCodeLength)]
    public required string CountryCode { get; set; }
    
    [Column("StateCode")]
    [MaxLength(DataConstants.StateCodeLength)]
    public string? StateCode { get; set; }
    
    [Column("CityId")]
    public uint CityId { get; set; }

    /// <summary>
    /// Current name of this venue
    /// </summary>
    [Column("CurrentName")]
    [MaxLength(DataConstants.VenueNameLength)]
    public required string CurrentName { get; set; }

    /// <summary>
    /// Previous names of this venue
    /// </summary>
    public ICollection<PreviousVenueNameDo> PreviousNames { get; set; } = [];
    
    /// <summary>
    /// Time Zone of this venue
    /// </summary>
    [Column("TimeZone")]
    [MaxLength(DataConstants.TimeZoneIdLength)]
    public required string TimeZone { get; set; }

    /// <summary>
    /// Latitude of the venue
    /// </summary>
    [Column("Latitude")]
    public decimal Latitude { get; set; }
    
    /// <summary>
    /// Longitude of the venue
    /// </summary>
    [Column("Longitude")]
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// Country where this venue is located in
    /// </summary>
    [ForeignKey(nameof(CountryCode))]
    public virtual CountryDo Country { get; set; }
    
    /// <summary>
    /// State where this venue is located in
    /// </summary>
    public virtual StateDo State { get; set; }
    
    /// <summary>
    /// City where this venue is located in
    /// </summary>
    public virtual CityDo City { get; set; }
}