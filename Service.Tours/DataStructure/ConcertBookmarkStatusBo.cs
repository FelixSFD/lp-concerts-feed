namespace Service.Tours.DataStructure;

/// <summary>
/// Contains information about the bookmarks for a concert
/// </summary>
public class ConcertBookmarkStatusBo
{
    /// <summary>
    /// Number of users who have bookmarked the concert
    /// </summary>
    public int Bookmarked { get; set; }
    
    /// <summary>
    /// Number of users who are attending the concert
    /// </summary>
    public int Attending { get; set; }
}