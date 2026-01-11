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
    [JsonPropertyName("sentAt")]
    public required DateTimeOffset SentAt { get; set; }

    [DynamoDBIgnore]
    [JsonIgnore]
    public required ConcertEventType EventType { get; set; }

    [DynamoDBProperty("Type")]
    [JsonPropertyName("type")]
    public string TypeString { get => EventType.ToString(); set => EventType = Enum.Parse<ConcertEventType>(value); }
}