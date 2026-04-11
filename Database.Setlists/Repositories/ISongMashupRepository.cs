using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISongMashupRepository : ISingleKeyRepositoryBase<SongMashupDo, uint>, IRepositoryBase<SongMashupDo>
{
    /// <summary>
    /// Returns all masups that match the <see cref="title"/>. This might return multiple results if we have duplicates.
    /// </summary>
    /// <param name="title">Title of the mashup</param>
    /// <returns>all mashups that were found</returns>
    public IAsyncEnumerable<SongMashupDo> GetMashupsByTitle(string title);
}