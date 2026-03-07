using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSongMashupRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SongMashupDo, uint>(dbContext, dbContext.SongMashups), ISongMashupRepository
{
}