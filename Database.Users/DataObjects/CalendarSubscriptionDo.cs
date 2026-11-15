using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Users.DataObjects;

/// <summary>
/// Represents a calendar subscription (for the concerts iCal).
/// It contains all settings to control what will be included in the feed.
/// </summary>
[Table("CalendarSubscription")]
[PrimaryKey(nameof(Id))]
public class CalendarSubscriptionDo : BaseDo, ITimestampedDataObject
{
    /// <summary>
    /// Unique ID of the subscription
    /// </summary>
    [Key]
    [Column("Id")]
    [MaxLength(DataConstants.CalendarSubscriptionIdLength)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the user who created this subscription. This makes managing the feed easier for the user.
    /// </summary>
    [Column("UserId")]
    [MaxLength(DataConstants.UserIdLength)]
    public string? UserId { get; set; }

    /// <summary>
    /// true, if the feed should include the time when the doors open
    /// </summary>
    [Column("IncludeDoorsTime")]
    public bool IncludeDoorsTime { get; set; }

    /// <summary>
    /// true, if the feed should include the stage time of LP
    /// </summary>
    [Column("IncludeLpStageTime")]
    public bool IncludeLpStageTime { get; set; }
    
    /// <inheritdoc/>
    [Column("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <inheritdoc/>
    [Column("UpdatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
    
    /// <summary>
    /// User who manages this subscription
    /// </summary>
    public virtual UserDo? User { get; set; }
}