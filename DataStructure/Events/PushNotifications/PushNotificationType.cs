namespace LPCalendar.DataStructure.Events.PushNotifications;

public enum PushNotificationType
{
    Custom,
    ConcertReminder,
    MainStageTimeConfirmed,
    /// <summary>
    /// Silent notification to inform the clients that they should run a new sync to get the latest data
    /// </summary>
    TriggerClientSync
}