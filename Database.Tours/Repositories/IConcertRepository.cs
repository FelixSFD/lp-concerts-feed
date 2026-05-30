using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface IConcertRepository : ISingleKeyRepositoryBase<ConcertDo, string>, IRepositoryBase<ConcertDo>
{
}