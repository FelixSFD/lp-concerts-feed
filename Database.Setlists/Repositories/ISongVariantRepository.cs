using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISongVariantRepository : ISingleKeyRepositoryBase<SongVariantDo, uint>, IRepositoryBase<SongVariantDo>
{
}