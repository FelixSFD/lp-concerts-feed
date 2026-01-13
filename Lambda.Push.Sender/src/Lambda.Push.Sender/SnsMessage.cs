using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

public class SnsMessage
{
    [JsonPropertyName("default")]
    public required string Default { get; set; }
    
    [JsonPropertyName("APNS")]
    public required string AppleNotificationService { get; set; }

    [JsonPropertyName("APNS_SANDBOX")]
    public string AppleNotificationServiceSandbox => AppleNotificationService;
}