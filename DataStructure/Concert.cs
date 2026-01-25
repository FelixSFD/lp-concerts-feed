using System.Text;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure.Converters;

namespace LPCalendar.DataStructure;

/// <summary>
/// Represents a concert of Linkin Park
/// </summary>
[DynamoDBTable(ConcertTableName)]
public class Concert
{
    public const string ConcertTableName = "Concertsv2";
    /// <summary>
    /// UUID of the concert
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    
    /// <summary>
    /// Type of show (like LP show, festival, ...)
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("showType")]
    public string ShowType { get; set; }
    
    
    /// <summary>
    /// Optional text to override the title of the show to display in the calendar
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("customTitle")]
    public string? CustomTitle { get; set; }
    
    
    [DynamoDBProperty]
    [JsonPropertyName("tourName")]
    public string? TourName { get; set; }

    /// <summary>
    /// Status of the concert (mainly used as workaround for having a partition key that can return all concerts for now)
    /// </summary>
    //[DynamoDBProperty("Status")]
    [DynamoDBGlobalSecondaryIndexHashKey("PostedStartTimeGlobalIndex")]
    [JsonPropertyName("status")]
    public required string Status { get; set; }
    

    /// <summary>
    /// Time when the concert starts according to Ticketmaster.
    /// </summary>
    [DynamoDBGlobalSecondaryIndexRangeKey("PostedStartTimeGlobalIndex")]
    [DynamoDBProperty("PostedStartTime", typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("postedStartTime")]
    public DateTimeOffset? PostedStartTime { get; set; }
    
    
    /// <summary>
    /// true, if LPU early entry has been confirmed for the show. Not all shows are guaranteed to have early entry.
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("lpuEarlyEntryConfirmed")]
    public bool LpuEarlyEntryConfirmed { get; set; }
    
    
    /// <summary>
    /// Time when doors will open for LPU early entry. If null, the time is probably not announced yet.
    /// </summary>
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("lpuEarlyEntryTime")]
    public DateTimeOffset? LpuEarlyEntryTime { get; set; }
    
    
    /// <summary>
    /// Time when the doors will open. If null, the time is probably not announced yet
    /// </summary>
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("doorsTime")]
    public DateTimeOffset? DoorsTime { get; set; }
    
    
    /// <summary>
    /// Time when Linkin Park is expected to enter the stage. If null, the time is probably not announced yet
    /// </summary>
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
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
    [DynamoDBProperty]
    [JsonPropertyName("timeZoneId")]
    public string TimeZoneId { get; set; }
    
    /// <summary>
    /// Country of the venue
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("country")]
    public string Country { get; set; }
    
    /// <summary>
    /// State where the venue is located in
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    /// <summary>
    /// City of the venue
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("city")]
    public string City { get; set; }
    
    /// <summary>
    /// Name of the venue
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("venue")]
    public string? Venue { get; set; }


    /// <summary>
    /// Long version of the location string (includes Venue, City, State, Country if available)
    /// </summary>
    [DynamoDBIgnore]
    [JsonPropertyName("locationLong")]
    public string LocationLong => LocationStringBuilder.GetLocationString(Venue, City, State, Country);

    
    /// <summary>
    /// Short version of the location string (includes Venue, City, Country if available)
    /// </summary>
    [DynamoDBIgnore]
    [JsonPropertyName("locationMedium")]
    public string LocationMedium => LocationStringBuilder.GetLocationString(Venue, City, null, Country);
    
    
    /// <summary>
    /// Short version of the location string (includes City, Country if available)
    /// </summary>
    [DynamoDBIgnore]
    [JsonPropertyName("locationShort")]
    public string LocationShort => LocationStringBuilder.GetLocationString(null, City, null, Country);


    /// <summary>
    /// Latitude of the venue
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("venueLatitude")]
    public decimal VenueLatitude { get; set; }
    
    
    /// <summary>
    /// Longitude of the venue
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("venueLongitude")]
    public decimal VenueLongitude { get; set; }
    
    
    /// <summary>
    /// Filename of the image that contains the show's schedule.
    /// It's possible that the name is set, but the image doesn't exist. In that case, the file was not uploaded (yet)
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("scheduleImageFile")]
    public string? ScheduleImageFile { get; set; }


    /// <summary>
    /// true, if the show was in the past (at the time of reading this property)
    /// A show is considered as "past", when the posted start time was more than 4 hour ago
    /// </summary>
    [DynamoDBIgnore]
    [JsonPropertyName("isPast")]
    public bool IsPast => PostedStartTime != null && PostedStartTime < DateTimeOffset.Now.AddHours(-4);
    
    
    /// <summary>
    /// Time when the concert was last edited
    /// </summary>
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("lastChange")]
    public DateTimeOffset? LastChange { get; set; }
}