using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.DataObjects;

/// <summary>
/// If a venue was renamed, this table stores the previous names
/// </summary>
[Table("PreviousVenueName")]
public class PreviousVenueNameDo : BaseDo, ITimestampedDataObject
{
    /// <summary>
    /// ID of this historic name
    /// </summary>
    [Column("Id")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }
    
    /// <summary>
    /// ID of the venue
    /// </summary>
    [Column("VenueId")]
    public uint VenueId { get; set; }
    
    /// <summary>
    /// Venue that used to have this name
    /// </summary>
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
    public DateOnly? To { get; set; }

    /// <inheritdoc/>
    [Column("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <inheritdoc/>
    [Column("UpdatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Name} {{ Name: {Name}, From: {From}, To: {To.ToString() ?? "infinity"} }}";
    }
}