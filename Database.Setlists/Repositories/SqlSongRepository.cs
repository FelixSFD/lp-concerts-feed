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
}