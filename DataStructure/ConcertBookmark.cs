namespace LPCalendar.DataStructure;

/// <summary>
/// Stores if a user has bookmarked a concert
/// </summary>
public class ConcertBookmark
{
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
    public string ConcertId { get; set; }
    
    
    /// <summary>
    /// ID of the user
    /// </summary>
    public string UserId { get; set; }
    
    
    /// <summary>
    /// Status of the bookmark
    /// </summary>
    public BookmarkStatus Status { get; set; }
}