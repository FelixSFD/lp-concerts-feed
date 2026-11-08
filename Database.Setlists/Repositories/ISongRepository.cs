using Common.Database.Repositories;
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
    
    /// <summary>
    /// Returns a dictionary of songs that were played at the given concerts, along with the number of times they were played.
    /// </summary>
    /// <param name="concertIds">IDs of the concerts to check</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    public Task<IDictionary<SongDo, uint>> GetSongsPlayedAtConcertsAsync(IEnumerable<string> concertIds,
        CancellationToken cancellationToken = default);
}