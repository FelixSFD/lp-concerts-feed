namespace LPCalendar.DataStructure.Events.PushNotifications;

/// <summary>
/// Event to publish to send a push notification for the concert.
/// Details of the event type must be sent in the messageAttributes in SQS
/// </summary>
public class ConcertRelatedPushNotificationEvent
{
    public required Concert Concert { get; set; }
}