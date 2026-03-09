using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISongRepository : ISingleKeyRepositoryBase<SongDo, uint>, IRepositoryBase<SongDo>
{
    /// <summary>
    /// Returns all songs with the given IDs
    /// </summary>
    /// <param name="songIds">IDs of the songs</param>
    /// <returns>all songs that were found</returns>
    public IAsyncEnumerable<SongDo> GetSongsByIds(params uint[] songIds);
}