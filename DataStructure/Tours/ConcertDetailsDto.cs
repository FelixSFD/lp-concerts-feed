using LPCalendar.DataStructure.Tours.Locations;

namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Detailed information about a concert
/// </summary>
public class ConcertDetailsDto
{
    /// <summary>
    /// Unique ID of the concert
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Type of the concert
    /// </summary>
    public required ConcertTypeDto ConcertType { get; set; }
    
    /// <summary>
    /// Information about the tour where this concert was played
    /// </summary>
    public TourDto? Tour { get; set; }
    
    /// <summary>
    /// Information about the leg in the <see cref="Tour"/>
    /// </summary>
    public TourLegDto? TourLeg { get; set; }

    /// <summary>
    /// Field to override the automatically generated title for this concert. Will only be used if it's not null
    /// </summary>
    public string? CustomTitle { get; set; }
    
    /// <summary>
    /// Information about the venue where this concert was played.
    /// This also contains the city and country.
    /// </summary>
    public required VenueWithDetailsDto Venue { get; set; }

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