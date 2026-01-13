using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

public class NotificationWrapper
{
    [JsonPropertyName("aps")]
    public required AppleNotificationAlert Apple { get; set; }
}