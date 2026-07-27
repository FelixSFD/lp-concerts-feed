using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface IConcertRepository : ISingleKeyRepositoryBase<ConcertDo, string>, IRepositoryBase<ConcertDo>
{
    /// <summary>
    /// Returns a (filtered) list of concerts
    /// </summary>
    /// <param name="token"></param>
    /// <param name="countryCode"></param>
    /// <param name="paginationParams"></param>
    /// <returns></returns>
    IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, string? countryCode = null,
        IPaginationParams? paginationParams = null);
}