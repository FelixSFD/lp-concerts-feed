using Common.Database.Repositories;
using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISetlistRepository : ISingleKeyRepositoryBase<SetlistDo, uint>, IRepositoryBase<SetlistDo>
{
    /// <summary>
    /// Retrieve all setlists for a given concert. While a concert usually only has one setlist, it can technically have more. (like soundchecks)
    /// </summary>
    /// <param name="concertId">ID of the concert</param>
    /// <returns>Setlist if one was found for the concert</returns>
    public IAsyncEnumerable<SetlistDo> GetByConcertIdAsync(string concertId);

    /// <summary>
    /// Returns the distinct IDs of concerts that have at least one setlist.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the query.</param>
    /// <returns>Concert IDs with setlists.</returns>
    public Task<List<string>> GetConcertIdsWithSetlistsAsync(CancellationToken cancellationToken = default);
}