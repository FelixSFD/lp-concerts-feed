namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Raw information about a concert
/// </summary>
public class RawConcertDto
{
    /// <summary>
    /// Unique ID of the concert
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// ID of the <see cref="ConcertTypeBo"/>
    /// </summary>
    public uint ConcertTypeId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="TourDto"/>
    /// </summary>
    public string? TourId { get; set; }
    
    /// <summary>
    /// ID of the <see cref="TourLegDto"/>
    /// </summary>
    public string? TourLegId { get; set; }

    /// <summary>
    /// Field to override the automatically generated title for this concert. Will only be used if it's not null
    /// </summary>
    public string? CustomTitle { get; set; }
    
    /// <summary>
    /// ID of the <see cref="VenueDto"/>
    /// </summary>
    public uint VenueId { get; set; }

    /// <summary>
    /// Start time as published on the ticket. This might not be the actual time when Linkin Park will be on stage.
    /// Detailed schedules are published closer to the concert.
    /// </summary>
    public DateTimeOffset PostedStartTime { get; set; }
    
    /// <summary>
    /// Time in the venue's timezone when Linkin Park will be on stage
    /// </summary>
    public DateTime? MainStageTime { get; set; }
    
    /// <summary>
    /// Time in the venue's timezone when the doors will open
    /// </summary>
    public DateTime? DoorsTime { get; set; }
    
    /// <summary>
    /// Time in the venue's timezone when the LPU Early Entry will start
    /// </summary>
    public DateTime? LpuEarlyEntryTime { get; set; }
    
    /// <summary>
    /// true, if LPU early entry has been confirmed for this concert
    /// </summary>
    public bool LpuEarlyEntryConfirmed { get; set; }
    
    /// <summary>
    /// Expected duration of the set in minutes
    /// </summary>
    public uint ExpectedSetDurationMinutes { get; set; }
    
    /// <summary>
    /// Name of the file in the S3 bucket that contains the official schedule for this concert
    /// </summary>
    public string? ScheduleImageFile { get; set; }
    
    /// <summary>
    /// Time when this concert was deleted
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Status of this concert
    /// </summary>
    public ConcertDto.ConcertStatusValue Status { get; set; }
}