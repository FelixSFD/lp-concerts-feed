using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISongVariantRepository : ISingleKeyRepositoryBase<SongVariantDo, uint>, IRepositoryBase<SongVariantDo>
{
    /// <summary>
    /// Returns all variants of a given song
    /// </summary>
    /// <param name="songId">ID of the song</param>
    /// <returns>all variants of that song</returns>
    public Task<List<SongVariantDo>> GetVariantsOfSongAsync(uint songId);
}