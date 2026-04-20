namespace LPCalendar.DataStructure.Events.PushNotifications;

/// <summary>
/// Payload of the notification that is sent when a song was premiered
/// </summary>
public class SetlistSongPremiereNotificationEvent
{
    /// <summary>
    /// Title of the setlist entry
    /// </summary>
    public required string SetlistEntryTitle { get; set; }
    
    /// <summary>
    /// The concert where the song was played
    /// </summary>
    public required ConcertDto Concert { get; set; }
}