using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface IConcertTypeRepository : ISingleKeyRepositoryBase<ConcertTypeDo, uint>, IRepositoryBase<ConcertTypeDo>
{
}