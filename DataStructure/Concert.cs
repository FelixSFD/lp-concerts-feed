using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure;

/// <summary>
/// Represents a concert of Linkin Park
/// </summary>
[DynamoDBTable("Concerts")]
public class Concert
{
    /// <summary>
    /// UUID of the concert
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Time when the concert starts according to Ticketmaster.
    /// </summary>
    private DateTimeOffset? _postedStartTime;

    /// <summary>
    /// Time when the concert starts according to Ticketmaster. As string for DynamoDB. Use <see cref="PostedStartTimeValue"/> to get the date-object.
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("postedStartTime")]
    public string? PostedStartTime
    {
        get => _postedStartTime.ToString(); // Serialize to ISO 8601 string
        set => _postedStartTime = string.IsNullOrEmpty(value) ? null : DateTimeOffset.Parse(value);
    }

    /// <summary>
    /// Time when the concert starts according to Ticketmaster.
    /// </summary>
    [DynamoDBIgnore]
    [JsonIgnore]
    public DateTimeOffset? PostedStartTimeValue
    {
        get => _postedStartTime;
        set => _postedStartTime = value;
    }
    
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
}