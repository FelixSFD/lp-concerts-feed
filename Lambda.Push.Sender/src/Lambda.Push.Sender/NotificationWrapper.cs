using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

public class NotificationWrapper<TApsType> where TApsType : AppleNotificationBase
{
    [JsonPropertyName("aps")]
    public required TApsType Apple { get; set; }


    [JsonPropertyName("concertId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ConcertId { get; set; }
    
    
    /// <summary>
    /// Set this to true if this is a background notification that should trigger a sync of the concerts on a client.
    /// </summary>
    [JsonPropertyName("triggerSync")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool TriggerSync { get; set; }
}