using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface IAlbumRepository : IRepositoryBase<AlbumDo>
{
    public Task<AlbumDo> GetByIdAsync(uint id);
}