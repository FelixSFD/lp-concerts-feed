using LPCalendar.DataStructure;

namespace Database.ConcertBookmarks;

public class InMemoryConcertBookmarkRepository : IConcertBookmarkRepository
{
    private readonly Dictionary<string, ConcertBookmark> _bookmarks = [];

    public void Add(ConcertBookmark bookmark)
    {
        _bookmarks[bookmark.Id] = bookmark;
    }
    
    public IAsyncEnumerable<ConcertBookmark> GetForUserAsync(string userId, ConcertBookmark.BookmarkStatus status)
    {
        return _bookmarks.Values.ToAsyncEnumerable().Where(bookmark => bookmark.UserId == userId);
    }
}