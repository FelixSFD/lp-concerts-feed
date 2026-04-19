using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure.Converters;

namespace LPCalendar.DataStructure.Entities;

/// <summary>
/// Stores the endpoint information for push notifications to users
/// </summary>
public class NotificationUserEndpoint
{
    public const string EndpointArnIndex = "EndpointArnIndex";
    public const string ReceiveConcertRemindersIndex = "ReceiveConcertRemindersIndex";
    public const string ReceiveMainStageTimeUpdatesIndex = "ReceiveMainStageTimeUpdatesIndex";
    
    public const string NoUser = "no_user";
    
    /// <summary>
    /// ID of the user to receive notifications
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("userId")]
    public required string UserId { get; set; }
    
    /// <summary>
    /// Device token of the device the user registered
    /// </summary>
    [DynamoDBRangeKey]
    [DynamoDBGlobalSecondaryIndexHashKey(EndpointArnIndex)]
    [JsonPropertyName("endpointArn")]
    public required string EndpointArn { get; set; }
    
    /// <summary>
    /// Time when the entry was last changed
    /// </summary>
    [DynamoDBProperty(typeof(DateTimeOffsetToStringPropertyConverter))]
    [JsonPropertyName("lastChange")]
    public DateTimeOffset? LastChange { get; set; }
    
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
    
    /// <summary>
    /// true if the user wants to receive an alert when a new song was played at a show.
    /// </summary>
    [JsonPropertyName("receiveSetlistSongPremiereAlerts")]
    public bool ReceiveSetlistSongPremiereAlerts { get; set; } = true;
}