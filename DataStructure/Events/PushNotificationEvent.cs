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
}