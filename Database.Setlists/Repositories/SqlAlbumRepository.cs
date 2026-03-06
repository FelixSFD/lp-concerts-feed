using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlAlbumRepository(SetlistsDbContext dbContext)
    : SqlRepositoryBase<AlbumDo>(dbContext, dbContext.Albums), IAlbumRepository
{
    public async Task<AlbumDo?> GetByIdAsync(uint id)
    {
        return await dbContext.Albums.FindAsync([id]);
    }
}