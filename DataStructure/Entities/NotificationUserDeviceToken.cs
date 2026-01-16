using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure.Converters;

namespace LPCalendar.DataStructure.Entities;

/// <summary>
/// Stores the endpoint information for push notifications to users
/// </summary>
public class NotificationUserEndpoint
{
    public const string EndpointArnIndex = "EndpointArnIndex";
    
    /// <summary>
    /// ID of the user to receive notifications
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }
    
    /// <summary>
    /// Device token of the device the user registered
    /// </summary>
    [DynamoDBRangeKey]
    [DynamoDBGlobalSecondaryIndexHashKey(EndpointArnIndex)]
    [JsonPropertyName("endpointArn")]
    public required string EndpointArn { get; set; }
    
    /// <summary>
    /// Time when the entry was last changed
    /// </summary>
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("lastChange")]
    public DateTimeOffset? LastChange { get; set; }
}