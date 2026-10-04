using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

/// <summary>
/// Repository to manage bookmarks for concerts.
/// </summary>
public interface IUserConcertBookmarkRepository : IRepositoryBase<UserConcertBookmarkDo>
{
    /// <summary>
    /// Returns the bookmark for the given user and concert.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="concertId">ID of the concert</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    Task<UserConcertBookmarkDo?> GetByUserIdAndConcertIdAsync(string userId, string concertId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns all bookmarks for the given concert.
    /// </summary>
    /// <param name="concertId">ID of the concert</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<UserConcertBookmarkDo>> GetByConcertId(string concertId, CancellationToken cancellationToken = default);
}