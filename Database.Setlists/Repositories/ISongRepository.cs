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
    
    /// <summary>
    /// Returns all songs that match the <see cref="title"/>. This might return multiple results if we have duplicates.
    /// </summary>
    /// <param name="title">Title of the song</param>
    /// <returns>all songs that were found</returns>
    public IAsyncEnumerable<SongDo> GetSongsByTitle(string title);
}