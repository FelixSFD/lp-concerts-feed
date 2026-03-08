using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

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
    
    /// <inheritdoc/>
    public async Task<List<SongVariantDo>> GetVariantsOfSongAsync(uint songId)
    {
        return await DbSet.AsQueryable()
            .Where(v => v.SongId == songId)
            .Include(v => v.Song)
            .ToListAsync();
    }
}