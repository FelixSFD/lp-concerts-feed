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
}