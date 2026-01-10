using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

public class AppleNotificationPayload
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}