using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;

namespace Database.Tours.DataObjects;

/// <summary>
/// All information about a concert
/// </summary>
[Table("Concert")]
public class ConcertDo : BaseDo
{
    /// <summary>
    /// Status of a concert
    /// </summary>
    public enum ConcertStatus
    {
        Planned,
        LiveRightNow,
        Past,
        Cancelled
    }
    
    /// <summary>
    /// Unique ID of this concert
    /// </summary>
    [Key]
    [Column("Id")]
    [MaxLength(DataConstants.ConcertIdLength)]
    public required string Id { get; set; }

    /// <summary>
    /// ID of the <see cref="ConcertTypeDo"/>
    /// </summary>
    [Column("ConcertTypeId")]
    public uint ConcertTypeId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="TourDo"/>
    /// </summary>
    [Column("TourId")]
    [MaxLength(DataConstants.TourIdLength)]
    public string? TourId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="TourLegDo"/>
    /// </summary>
    [Column("TourLegId")]
    [MaxLength(DataConstants.TourIdLength)]
    public string? TourLegId { get; set; }

    /// <summary>
    /// Field to override the automatically generated title for this concert. Will only be used if it's not null
    /// </summary>
    [Column("CustomTitle")]
    [MaxLength(DataConstants.TitleFieldLength)]
    public string? CustomTitle { get; set; }
    
    /// <summary>
    /// ID of the <see cref="VenueDo"/>
    /// </summary>
    [Column("VenueId")]
    public uint VenueId { get; set; }

    /// <summary>
    /// Start time as published on the ticket. This might not be the actual time when Linkin Park will be on stage.
    /// Detailed schedules are published closer to the concert.
    /// </summary>
    [Column("PostedStartTime")]
    public DateTimeOffset PostedStartTime { get; set; }
    
    /// <summary>
    /// Time in the venue's timezone when Linkin Park will be on stage
    /// </summary>
    [Column("MainStageTime")]
    public DateTime? MainStageTime { get; set; }
    
    /// <summary>
    /// Time in the venue's timezone when the doors will open
    /// </summary>
    [Column("DoorsTime")]
    public DateTime? DoorsTime { get; set; }
    
    /// <summary>
    /// Time in the venue's timezone when the LPU Early Entry will start
    /// </summary>
    [Column("LpuEarlyEntryTime")]
    public DateTime? LpuEarlyEntryTime { get; set; }
    
    /// <summary>
    /// true, if LPU early entry has been confirmed for this concert
    /// </summary>
    [Column("LpuEarlyEntryConfirmed")]
    public bool LpuEarlyEntryConfirmed { get; set; }
    
    /// <summary>
    /// Expected duration of the set in minutes
    /// </summary>
    [Column("ExpectedSetDurationMinutes")]
    public uint ExpectedSetDurationMinutes { get; set; }
    
    /// <summary>
    /// Name of the file in the S3 bucket that contains the official schedule for this concert
    /// </summary>
    [Column("ScheduleImageFile")]
    [MaxLength(DataConstants.ConcertScheduleFileNameLength)]
    public string? ScheduleImageFile { get; set; }
    
    /// <summary>
    /// Time when this concert was deleted
    /// </summary>
    [Column("DeletedAt")]
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Status of this concert
    /// </summary>
    [Column("Status")]
    public ConcertStatus Status { get; set; }

    /// <summary>
    /// Tour this concert is a part of
    /// </summary>
    [ForeignKey(nameof(TourId))]
    public virtual TourDo? Tour { get; set; }
    
    /// <summary>
    /// Leg of a tour this concert is a part of
    /// </summary>
    public virtual TourLegDo? TourLeg { get; set; }
    
    /// <summary>
    /// Type of this concert
    /// </summary>
    [ForeignKey(nameof(ConcertTypeId))]
    public virtual ConcertTypeDo Type { get; set; }
    
    /// <summary>
    /// Venue where this concert was played
    /// </summary>
    [ForeignKey(nameof(VenueId))]
    public virtual VenueDo Venue { get; set; }
}