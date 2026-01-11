namespace LPCalendar.DataStructure.Events.PushNotifications;

public class ConcertRelatedPushNotificationEvent
{
    //public PushNotificationType Type { get; set; }


    public required Concert Concert { get; set; }
}