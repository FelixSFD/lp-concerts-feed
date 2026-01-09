using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure.Entities;

/// <summary>
/// Stores the endpoint information for push notifications to users
/// </summary>
public class NotificationRegistration
{
    /// <summary>
    /// ID of the user to receive notifications
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }
    
    /// <summary>
    /// Notification endpoint
    /// </summary>
    [DynamoDBRangeKey]
    [JsonPropertyName("endpoint")]
    public required string Endpoint { get; set; }
}