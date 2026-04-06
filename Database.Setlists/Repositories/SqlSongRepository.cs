using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSongRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SongDo, uint>(dbContext, dbContext.Songs), ISongRepository
{
    /// <inheritdoc/>
    protected override async Task<SongDo> LoadReferences(SongDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(s => s.Album)
            .LoadAsync();
        
        return dataObject;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<SongDo> GetSongsByIds(params uint[] songIds)
    {
        var idList = songIds.ToList();
        return DbSet
            .AsQueryable()
            .Where(s => idList.Contains(s.Id))
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<SongDo> GetSongsByTitle(string title)
    {
        return DbSet
            .AsQueryable()
            .Where(s => s.Title == title)
            .ToAsyncEnumerable();
    }
}