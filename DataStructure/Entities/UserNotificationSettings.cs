using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure.Entities;

/// <summary>
/// Stores the notification settings for a user
/// </summary>
public class UserNotificationSettings
{
    public const string ReceiveConcertRemindersIndex = "ReceiveConcertRemindersIndex";
    public const string ReceiveMainStageTimeUpdatesIndex = "ReceiveMainStageTimeUpdatesIndex";
    
    
    /// <summary>
    /// ID of the user
    /// </summary>
    [DynamoDBHashKey]
    [DynamoDBGlobalSecondaryIndexHashKey(ReceiveConcertRemindersIndex, ReceiveMainStageTimeUpdatesIndex)]
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    /// <summary>
    /// true if the user wants to receive concert reminders.
    /// <see cref="ConcertRemindersStatus"/> controls for which concerts reminders are sent
    /// </summary>
    [JsonPropertyName("receiveConcertReminders")]
    public bool ReceiveConcertReminders { get; set; } = true;

    /// <summary>
    /// Bookmark status required to receive reminders for that concert
    /// </summary>
    [JsonIgnore]
    [DynamoDBIgnore]
    public IEnumerable<ConcertBookmark.BookmarkStatus> ConcertRemindersStatus { get; set; } = [ConcertBookmark.BookmarkStatus.None, ConcertBookmark.BookmarkStatus.Bookmarked, ConcertBookmark.BookmarkStatus.Attending];

    [DynamoDBProperty(nameof(ConcertRemindersStatus))]
    [DynamoDBGlobalSecondaryIndexHashKey(ReceiveConcertRemindersIndex)]
    [JsonPropertyName("concertRemindersStatus")]
    public string[] ConcertRemindersStatusStrings
    {
        get => ConcertRemindersStatus.Select(s => s.ToString()).ToArray();
        set => ConcertRemindersStatus = value.Select(Enum.Parse<ConcertBookmark.BookmarkStatus>);
    }

    /// <summary>
    /// true if the user wants to receive a notification when the <see cref="Concert.MainStageTime"/> is updated.
    /// <see cref="MainStageTimeUpdatesStatus"/> controls for which concerts reminders are sent
    /// </summary>
    [JsonPropertyName("receiveMainStageTimeUpdates")]
    public bool ReceiveMainStageTimeUpdates { get; set; } = true;
    
    /// <summary>
    /// Bookmark status required to receive reminders for that concert
    /// </summary>
    [JsonIgnore]
    [DynamoDBIgnore]
    public IEnumerable<ConcertBookmark.BookmarkStatus> MainStageTimeUpdatesStatus { get; set; } = [ConcertBookmark.BookmarkStatus.None, ConcertBookmark.BookmarkStatus.Bookmarked, ConcertBookmark.BookmarkStatus.Attending];
    
    [DynamoDBProperty(nameof(MainStageTimeUpdatesStatus))]
    [DynamoDBGlobalSecondaryIndexHashKey(ReceiveMainStageTimeUpdatesIndex)]
    [JsonPropertyName("mainStageTimeUpdatesStatus")]
    public string[] MainStageTimeUpdatesStatusStrings
    {
        get => MainStageTimeUpdatesStatus.Select(s => s.ToString()).ToArray();
        set => MainStageTimeUpdatesStatus = value.Select(Enum.Parse<ConcertBookmark.BookmarkStatus>);
    }
}