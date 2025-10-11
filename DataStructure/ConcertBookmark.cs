using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure;

/// <summary>
/// Stores if a user has bookmarked a concert
/// </summary>
[DynamoDBTable(ConcertBookmarkTableName)]
public class ConcertBookmark
{
    public const string ConcertBookmarkTableName = "ConcertBookmarks";
    public const string UserBookmarksIndex = "UserBookmarksIndexV1";
    public const string ConcertBookmarkStatusIndex = "ConcertBookmarkStatusIndexV1";
    
    /// <summary>
    /// Status of the bookmark
    /// </summary>
    public enum BookmarkStatus
    {
        /// <summary>
        /// not bookmarked
        /// </summary>
        None,
        
        /// <summary>
        /// Bookmarked. Just interested in that show
        /// </summary>
        Bookmarked,
        
        /// <summary>
        /// User will attend the show (or did attend)
        /// </summary>
        Attending
    }
    
    
    /// <summary>
    /// ID of the bookmark (exists for technical reasons)
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    
    /// <summary>
    /// ID of the concert
    /// </summary>
    [DynamoDBGlobalSecondaryIndexHashKey(ConcertBookmarkStatusIndex)]
    [DynamoDBGlobalSecondaryIndexRangeKey(UserBookmarksIndex)]
    [DynamoDBProperty]
    [JsonPropertyName("concertId")]
    public string ConcertId { get; set; }
    
    
    /// <summary>
    /// ID of the user
    /// </summary>
    [DynamoDBGlobalSecondaryIndexHashKey(UserBookmarksIndex, "UserBookmarkStatusIndexV1")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    
    
    /// <summary>
    /// Status of the bookmark
    /// </summary>
    [DynamoDBIgnore]
    [JsonIgnore]
    public BookmarkStatus Status { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("UserBookmarkStatusIndexV1", ConcertBookmarkStatusIndex)]
    [DynamoDBProperty("Status")]
    [JsonPropertyName("status")]
    public string StatusString
    {
        get => Status.ToString();
        set => Status = Enum.Parse<BookmarkStatus>(value);
    }
}