using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface IAlbumRepository : ISingleKeyRepositoryBase<AlbumDo, uint>, IRepositoryBase<AlbumDo>
{
}