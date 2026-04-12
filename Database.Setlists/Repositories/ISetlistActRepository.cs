using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISetlistActRepository : IRepositoryBase<SetlistActDo>
{
    /// <summary>
    /// Retrieve an Act by its setlist ID and act number
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="actNumber"></param>
    /// <returns></returns>
    public Task<SetlistActDo?> GetBy(uint setlistId, uint actNumber);
}