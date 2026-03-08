using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlAlbumRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<AlbumDo, uint>(dbContext, dbContext.Albums), IAlbumRepository
{
    /// <inheritdoc/>
    protected override Task<AlbumDo> LoadReferences(AlbumDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}