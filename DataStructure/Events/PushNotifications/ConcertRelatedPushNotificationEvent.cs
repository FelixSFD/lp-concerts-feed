namespace LPCalendar.DataStructure.Events.PushNotifications;

public class ConcertRelatedPushNotificationEvent
{
    //public ConcertEventType Type { get; set; }


    public required Concert Concert { get; set; }
}