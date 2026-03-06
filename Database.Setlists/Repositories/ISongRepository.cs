using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISongRepository : ISingleKeyRepositoryBase<SongDo, uint>, IRepositoryBase<SongDo>
{
}