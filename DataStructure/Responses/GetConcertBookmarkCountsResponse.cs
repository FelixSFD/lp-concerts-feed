using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

/// <summary>
/// Response from server to tell the number of people who bookmarked or attended a show
/// </summary>
public class GetConcertBookmarkCountsResponse
{
    /// <summary>
    /// Number of users who set a bookmark
    /// </summary>
    [JsonPropertyName("bookmarked")]
    public int Bookmarked { get; set; }
    
    /// <summary>
    /// Number of users who set their status to "Attending"
    /// </summary>
    [JsonPropertyName("attending")]
    public int Attending { get; set; }

    /// <summary>
    /// Bookmark status of the current user, if logged in
    /// </summary>
    [JsonPropertyName("currentUserStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ConcertBookmark.BookmarkStatus CurrentUserStatus { get; set; }
}