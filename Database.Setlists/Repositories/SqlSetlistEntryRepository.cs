using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSetlistEntryRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SetlistEntryDo, string>(dbContext, dbContext.SetlistEntries), ISetlistEntryRepository
{
    /// <inheritdoc/>
    protected override async Task<SetlistEntryDo> LoadReferences(SetlistEntryDo dataObject)
    {
        await Context.Entry(dataObject)
            .Collection(e => e.SongExtras)
            .LoadAsync();
        
        await Context.Entry(dataObject)
            .Reference(e => e.Act)
            .LoadAsync();
        
        await Context.Entry(dataObject)
            .Reference(e => e.PlayedSong)
            .LoadAsync();
        
        await Context.Entry(dataObject)
            .Reference(e => e.Setlist)
            .LoadAsync();
        
        return dataObject;
    }
}