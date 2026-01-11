using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.Events;

namespace LPCalendar.DataStructure.Entities;

/// <summary>
/// Notification that was sent about a concert
/// </summary>
public class ConcertNotificationHistory
{
    [DynamoDBHashKey]
    public required string ConcertId { get; set; }
    
    [DynamoDBProperty("SentAt", typeof(DateTimeOffsetToStringPropertyConverter))]
    [DynamoDBRangeKey]
    [JsonPropertyName("mainStageTime")]
    public required DateTimeOffset SentAt { get; set; }

    public required ConcertEventType Type { get; set; }
}