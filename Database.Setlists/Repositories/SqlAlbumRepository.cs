using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlAlbumRepository : SqlRepositoryBase<AlbumDo>, IAlbumRepository
{
    public Task<AlbumDo> GetByIdAsync(uint id)
    {
        throw new NotImplementedException();
    }
}