using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISongMashupRepository : ISingleKeyRepositoryBase<SongMashupDo, uint>, IRepositoryBase<SongMashupDo>
{
}