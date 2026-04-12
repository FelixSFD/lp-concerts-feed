using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists;

namespace LPCalendar.DataStructure;

/// <summary>
/// Represents a concert of Linkin Park
/// </summary>
public class ConcertDto
{
    /// <summary>
    /// UUID of the concert
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    
    /// <summary>
    /// Type of show (like LP show, festival, ...)
    /// </summary>
    [JsonPropertyName("showType")]
    public string ShowType { get; set; }
    
    
    /// <summary>
    /// Optional text to override the title of the show to display in the calendar
    /// </summary>
    [JsonPropertyName("customTitle")]
    public string? CustomTitle { get; set; }
    
    
    [JsonPropertyName("tourName")]
    public string? TourName { get; set; }

    /// <summary>
    /// Status of the concert (mainly used as workaround for having a partition key that can return all concerts for now)
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    

    /// <summary>
    /// Time when the concert starts according to Ticketmaster.
    /// </summary>
    [JsonPropertyName("postedStartTime")]
    public DateTimeOffset? PostedStartTime { get; set; }
    
    
    /// <summary>
    /// true, if LPU early entry has been confirmed for the show. Not all shows are guaranteed to have early entry.
    /// </summary>
    [JsonPropertyName("lpuEarlyEntryConfirmed")]
    public bool LpuEarlyEntryConfirmed { get; set; }
    
    
    /// <summary>
    /// Time when doors will open for LPU early entry. If null, the time is probably not announced yet.
    /// </summary>
    [JsonPropertyName("lpuEarlyEntryTime")]
    public DateTimeOffset? LpuEarlyEntryTime { get; set; }
    
    
    /// <summary>
    /// Time when the doors will open. If null, the time is probably not announced yet
    /// </summary>
    [JsonPropertyName("doorsTime")]
    public DateTimeOffset? DoorsTime { get; set; }
    
    
    /// <summary>
    /// Time when Linkin Park is expected to enter the stage. If null, the time is probably not announced yet
    /// </summary>
    [JsonPropertyName("mainStageTime")]
    public DateTimeOffset? MainStageTime { get; set; }


    /// <summary>
    /// Expected duration in minutes of the Linkin Park set during this event
    /// </summary>
    [JsonPropertyName("expectedSetDuration")]
    public short? ExpectedSetDuration { get; set; }

    
    /// <summary>
    /// Timezone of the venue
    /// </summary>
    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId { get; set; }
    
    /// <summary>
    /// Country of the venue
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; }
    
    /// <summary>
    /// State where the venue is located in
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    /// <summary>
    /// City of the venue
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; }
    
    /// <summary>
    /// Name of the venue
    /// </summary>
    [JsonPropertyName("venue")]
    public string? Venue { get; set; }


    /// <summary>
    /// Long version of the location string (includes Venue, City, State, Country if available)
    /// </summary>
    [JsonPropertyName("locationLong")]
    public string LocationLong => LocationStringBuilder.GetLocationString(Venue, City, State, Country);

    
    /// <summary>
    /// Short version of the location string (includes Venue, City, Country if available)
    /// </summary>
    [JsonPropertyName("locationMedium")]
    public string LocationMedium => LocationStringBuilder.GetLocationString(Venue, City, null, Country);
    
    
    /// <summary>
    /// Short version of the location string (includes City, Country if available)
    /// </summary>
    [JsonPropertyName("locationShort")]
    public string LocationShort => LocationStringBuilder.GetLocationString(null, City, null, Country);


    /// <summary>
    /// Latitude of the venue
    /// </summary>
    [JsonPropertyName("venueLatitude")]
    public decimal VenueLatitude { get; set; }
    
    
    /// <summary>
    /// Longitude of the venue
    /// </summary>
    [JsonPropertyName("venueLongitude")]
    public decimal VenueLongitude { get; set; }
    
    
    /// <summary>
    /// Filename of the image that contains the show's schedule.
    /// It's possible that the name is set, but the image doesn't exist. In that case, the file was not uploaded (yet)
    /// </summary>
    [JsonPropertyName("scheduleImageFile")]
    public string? ScheduleImageFile { get; set; }


    /// <summary>
    /// true, if the show was in the past (at the time of reading this property)
    /// A show is considered as "past", when the posted start time was more than 4 hour ago
    /// </summary>
    [JsonPropertyName("isPast")]
    public bool IsPast => PostedStartTime != null && PostedStartTime < DateTimeOffset.Now.AddHours(-4);
    
    
    /// <summary>
    /// Time when the concert was last edited
    /// </summary>
    [JsonPropertyName("lastChange")]
    public DateTimeOffset? LastChange { get; set; }
    
    
    /// <summary>
    /// Time when the concert was deleted
    /// </summary>
    [JsonPropertyName("deletedAt")]
    public DateTimeOffset? DeletedAt { get; set; }
}

/// <summary>
/// Extends the <see cref="ConcertDto"/> with setlist information.
/// </summary>
public class ConcertWithSetlistsDto : ConcertDto
{
    /// <summary>
    /// Cached setlists for this concert.
    /// This is used as a cache so we don't have to hit the SQL DB all the time
    /// </summary>
    [JsonPropertyName("cachedSetlists")]
    public required List<SetlistDto> CachedSetlists { get; set; }

    /// <summary>
    /// Time when the <see cref="CachedSetlists"/> was last updated
    /// </summary>
    [JsonPropertyName("cachedSetlistsAt")]
    public DateTimeOffset? CachedSetlistsAt { get; set; }
}