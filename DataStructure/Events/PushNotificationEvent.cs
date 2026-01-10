namespace LPCalendar.DataStructure.Events;

public class PushNotificationEvent
{
    /// <summary>
    /// User to send the notification to
    /// </summary>
    public required string UserId { get; set; }

    public required string Title { get; set; }
    public required string Body { get; set; }
}