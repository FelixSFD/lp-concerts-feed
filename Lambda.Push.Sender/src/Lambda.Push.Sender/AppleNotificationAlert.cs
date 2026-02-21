using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

public class AppleNotificationBase
{
}

public class AppleNotificationAlert : AppleNotificationBase
{
    public const string DefaultServerFilteredCategory = "default";
    
    [JsonPropertyName("alert")]
    public required AppleNotificationPayload Alert { get; set; }

    [JsonPropertyName("thread-id")]
    public string? ThreadId { get; set; }

    /// <summary>
    /// Use <see cref="DefaultServerFilteredCategory"/> if this notification doesn't need a category as the server already filtered it.
    /// </summary>
    [JsonPropertyName("category")]
    public required string Category { get; set; }
    
    [JsonIgnore]
    public bool MutableContent { get; set; }
    
    [JsonPropertyName("mutable-content")]
    public int MutableContentAsInt { get => MutableContent ? 1 : 0; set => MutableContent = value == 1; }
}