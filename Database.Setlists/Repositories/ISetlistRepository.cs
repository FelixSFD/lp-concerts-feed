using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISetlistRepository : ISingleKeyRepositoryBase<SetlistDo, uint>, IRepositoryBase<SetlistDo>
{
    /// <summary>
    /// Retrieve a setlist for a given concert.
    /// </summary>
    /// <param name="concertId">ID of the concert</param>
    /// <returns>Setlist if one was found for the concert</returns>
    public Task<SetlistDo?> GetByConcertIdAsync(string concertId);
}