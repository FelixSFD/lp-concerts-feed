using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

public class AppleNotificationAlert
{
    [JsonPropertyName("alert")]
    public required AppleNotificationPayload Alert { get; set; }

    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }
}