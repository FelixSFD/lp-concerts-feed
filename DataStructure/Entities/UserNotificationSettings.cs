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
    
    [DynamoDBIgnore]
    [JsonPropertyName("concertRemindersStatus")]
    public string[] ConcertRemindersStatusStrings
    {
        get => ConcertRemindersStatus.Select(s => s.ToString()).ToArray();
        set => ConcertRemindersStatus = value.Where(s => !string.IsNullOrEmpty(s)).Select(Enum.Parse<ConcertBookmark.BookmarkStatus>);
    }
    
    /// <summary>
    /// Wrapper for <see cref="ConcertRemindersStatus"/> for DynamoDB. String Sets cannot be empty.
    /// </summary>
    [DynamoDBProperty(nameof(ConcertRemindersStatus))]
    [DynamoDBGlobalSecondaryIndexHashKey(ReceiveConcertRemindersIndex)]
    [JsonIgnore]
    public string[] ConcertRemindersStatusStringsDynamoDb
    {
        get
        {
            var tmpArray = ConcertRemindersStatusStrings;
            return tmpArray.Length == 0 ? [""] : tmpArray;
        }
        set => ConcertRemindersStatusStrings = value;
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
    
    [DynamoDBIgnore]
    [JsonPropertyName("mainStageTimeUpdatesStatus")]
    public string[] MainStageTimeUpdatesStatusStrings
    {
        get => MainStageTimeUpdatesStatus.Select(s => s.ToString()).ToArray();
        set => MainStageTimeUpdatesStatus = value.Where(s => !string.IsNullOrEmpty(s)).Select(Enum.Parse<ConcertBookmark.BookmarkStatus>);
    }
    
    /// <summary>
    /// Wrapper for <see cref="MainStageTimeUpdatesStatus"/> for DynamoDB. String Sets cannot be empty.
    /// </summary>
    [DynamoDBProperty(nameof(MainStageTimeUpdatesStatus))]
    [DynamoDBGlobalSecondaryIndexHashKey(ReceiveMainStageTimeUpdatesIndex)]
    [JsonIgnore]
    public string[] MainStageTimeUpdatesStatusStringsDynamoDb
    {
        get
        {
            var tmpArray = MainStageTimeUpdatesStatusStrings;
            return tmpArray.Length == 0 ? [""] : tmpArray;
        }
        set => MainStageTimeUpdatesStatusStrings = value;
    }
}