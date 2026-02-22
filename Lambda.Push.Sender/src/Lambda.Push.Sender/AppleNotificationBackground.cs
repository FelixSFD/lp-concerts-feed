using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

/// <summary>
/// Background Notification for Apple devices. Is used to inform the clients about updates, so they can sync
/// </summary>
public class AppleNotificationBackground: AppleNotificationBase
{
    [JsonIgnore]
    public bool ContentAvailable { get; set; } = true;
    
    [JsonPropertyName("content-available")]
    public int ContentAvailableAsInt { get => ContentAvailable ? 1 : 0; set => ContentAvailable = value == 1; }
}