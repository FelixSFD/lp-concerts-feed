using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

/// <summary>
/// If a venue was renamed, this table stores the previous names
/// </summary>
[Table("PreviousVenueName")]
public class PreviousVenueNameDo : BaseDo
{
    [Column("VenueId")]
    public uint VenueId { get; set; }
    
    /// <summary>
    /// Venue that used to have this name
    /// </summary>
    [ForeignKey(nameof(VenueId))]
    public virtual VenueDo Venue { get; set; }
    
    /// <summary>
    /// name of this venue during the timeframe
    /// </summary>
    [Column("Name")]
    [MaxLength(DataConstants.VenueNameLength)]
    public required string Name { get; set; }

    /// <summary>
    /// Since when the venue had this name
    /// </summary>
    [Column("From")]
    public DateOnly From { get; set; }
    
    /// <summary>
    /// The venue got a new name after this day
    /// </summary>
    [Column("To")]
    public DateOnly To { get; set; }
}