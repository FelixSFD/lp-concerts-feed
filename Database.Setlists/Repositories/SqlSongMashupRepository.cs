using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSongMashupRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SongMashupDo, uint>(dbContext, dbContext.SongMashups), ISongMashupRepository
{
    /// <inheritdoc/>
    protected override async Task<SongMashupDo> LoadReferences(SongMashupDo dataObject)
    {
        await Context.Entry(dataObject)
            .Collection(e => e.Songs)
            .LoadAsync();
        
        return dataObject;
    }
}