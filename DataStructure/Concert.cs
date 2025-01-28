﻿using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

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
    [DynamoDBProperty("PostedStartTime")]
    [JsonPropertyName("postedStartTime")]
    public DateTimeOffset? PostedStartTime { get; set; }
    
    
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
}