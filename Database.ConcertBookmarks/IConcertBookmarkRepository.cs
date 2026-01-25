using LPCalendar.DataStructure;

namespace Database.ConcertBookmarks;

/// <summary>
/// Repository to manage bookmarks for concerts
/// </summary>
public interface IConcertBookmarkRepository
{
    /// <summary>
    /// Returns all bookmarks with a given status for the user.
    /// </summary>
    /// <param name="userId">UserID</param>
    /// <param name="status">Status</param>
    /// <returns></returns>
    public IAsyncEnumerable<ConcertBookmark> GetForUserAsync(string userId, ConcertBookmark.BookmarkStatus status);
}