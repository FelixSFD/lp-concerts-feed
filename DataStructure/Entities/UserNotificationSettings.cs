using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure.Converters;

namespace LPCalendar.DataStructure.Entities;

/// <summary>
/// Stores the notification settings for a user
/// </summary>
public class UserNotificationSettings
{
    /// <summary>
    /// ID of the user
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }

    [DynamoDBRangeKey]
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("lastUpdated")]
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// true if the user wants to receive concert reminders.
    /// <see cref="ConcertRemindersStatus"/> controls for which concerts reminders are sent
    /// </summary>
    [JsonPropertyName("receiveConcertReminders")]
    public bool ReceiveConcertReminders { get; set; } = true;

    /// <summary>
    /// Bookmark status required to receive reminders for that concert
    /// </summary>
    [JsonPropertyName("concertRemindersStatus")]
    public IEnumerable<ConcertBookmark.BookmarkStatus> ConcertRemindersStatus { get; set; } = [ConcertBookmark.BookmarkStatus.None, ConcertBookmark.BookmarkStatus.Bookmarked, ConcertBookmark.BookmarkStatus.Attending];

    /// <summary>
    /// true if the user wants to receive a notification when the <see cref="Concert.MainStageTime"/> is updated.
    /// <see cref="MainStageTimeUpdatesStatus"/> controls for which concerts reminders are sent
    /// </summary>
    [JsonPropertyName("receiveMainStageTimeUpdates")]
    public bool ReceiveMainStageTimeUpdates { get; set; } = true;
    
    /// <summary>
    /// Bookmark status required to receive reminders for that concert
    /// </summary>
    [JsonPropertyName("mainStageTimeUpdatesStatus")]
    public IEnumerable<ConcertBookmark.BookmarkStatus> MainStageTimeUpdatesStatus { get; set; } = [ConcertBookmark.BookmarkStatus.None, ConcertBookmark.BookmarkStatus.Bookmarked, ConcertBookmark.BookmarkStatus.Attending];
}