namespace LPCalendar.DataStructure.Events;

public class PushNotificationEvent
{
    /// <summary>
    /// Constant to use as UserId for sending a broadcast to all registered devices
    /// </summary>
    public const string AnyUser = "broadcast";
    
    /// <summary>
    /// User to send the notification to
    /// </summary>
    public required string UserId { get; set; }

    public required string Title { get; set; }
    public required string Body { get; set; }

    /// <summary>
    /// Identifier that can be used to replace notifications.
    /// Set this if sending a new message with the same ID should replace the previous one
    /// </summary>
    public string? CollapseId { get; set; }
    
    /// <summary>
    /// Push notifications with the same thread will be grouped in the notification center (APNS Thread)
    /// </summary>
    public string? Thread { get; set; }
}