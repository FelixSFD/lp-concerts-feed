using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure;

/// <summary>
/// Stores if a user has bookmarked a concert
/// </summary>
public class ConcertBookmark
{
    public const string ConcertBookmarkTableName = "ConcertBookmarksV1";
    
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
    /// ID of the concert
    /// </summary>
    [DynamoDBHashKey]
    [JsonPropertyName("concertId")]
    public string ConcertId { get; set; }
    
    
    /// <summary>
    /// ID of the user
    /// </summary>
    [DynamoDBGlobalSecondaryIndexHashKey("UserBookmarksIndex")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    
    
    /// <summary>
    /// Status of the bookmark
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("status")]
    public BookmarkStatus Status { get; set; }
}