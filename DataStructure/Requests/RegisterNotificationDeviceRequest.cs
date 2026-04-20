using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Requests;

/// <summary>
/// Request from API gateway to register a device to a user for push notifications
/// </summary>
public class RegisterNotificationDeviceRequest
{
    /// <summary>
    /// ID of the user
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
    
    /// <summary>
    /// Device token issued by Apple
    /// </summary>
    [JsonPropertyName("deviceToken")]
    public required string DeviceToken { get; set; }
    
    /// <summary>
    /// true if the user wants to receive concert reminders.
    /// <see cref="ConcertRemindersStatusStrings"/> controls for which concerts reminders are sent
    /// </summary>
    [JsonPropertyName("receiveConcertReminders")]
    public bool ReceiveConcertReminders { get; set; } = true;

    [JsonPropertyName("concertRemindersStatus")]
    public string[]? ConcertRemindersStatusStrings { get; set; }

    /// <summary>
    /// true if the user wants to receive a notification when the <see cref="Concert.MainStageTime"/> is updated.
    /// <see cref="MainStageTimeUpdatesStatusStrings"/> controls for which concerts reminders are sent
    /// </summary>
    [JsonPropertyName("receiveMainStageTimeUpdates")]
    public bool ReceiveMainStageTimeUpdates { get; set; } = true;

    [JsonPropertyName("mainStageTimeUpdatesStatus")]
    public string[]? MainStageTimeUpdatesStatusStrings { get; set; }
    
    /// <summary>
    /// true if the user wants to receive an alert when a new song was played at a show.
    /// </summary>
    [JsonPropertyName("receiveSetlistSongPremiereAlerts")]
    public bool ReceiveSetlistSongPremiereAlerts { get; set; } = true;
}