using System.ComponentModel.DataAnnotations.Schema;
using Common.Database.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.DataObjects;

/// <summary>
/// Information about a bookmark for a concert by a user. This can be used to track whether a user has bookmarked a concert or is attending it.
/// </summary>
[PrimaryKey(nameof(UserId), nameof(ConcertId))]
[Table("UserConcertBookmark")]
public class UserConcertBookmarkDo : BaseDo
{
    /// <summary>
    /// ID of the user
    /// </summary>
    public required string UserId { get; set; }
    
    /// <summary>
    /// ID of the concert
    /// </summary>
    public required string ConcertId { get; set; }

    /// <summary>
    /// Status of the user's bookmark for a concert.
    /// </summary>
    public BookmarkStatus Status { get; set; }

    /// <summary>
    /// The concert that the user has bookmarked or is attending.
    /// </summary>
    public virtual ConcertDo Concert { get; set; }
    
    /// <summary>
    /// Status of the user's bookmark for a concert.
    /// </summary>
    public enum BookmarkStatus
    {
        None = 0,
        Bookmarked = 1,
        Attending = 2,
    }
}