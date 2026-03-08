using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSongVariantRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SongVariantDo, uint>(dbContext, dbContext.SongVariants), ISongVariantRepository
{
    /// <inheritdoc/>
    protected override async Task<SongVariantDo> LoadReferences(SongVariantDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(s => s.Song)
            .LoadAsync();
        
        return dataObject;
    }
}