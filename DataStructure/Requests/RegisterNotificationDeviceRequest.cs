using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Requests;

/// <summary>
/// Request from API gateway to register a device to a user for push notifications
/// </summary>
public class RegisterNotificationDeviceRequest
{
    /// <summary>
    /// ID of the user
    /// </summary>
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }
    
    /// <summary>
    /// Device token issued by Apple
    /// </summary>
    [JsonPropertyName("deviceToken")]
    public required string DeviceToken { get; set; }
}