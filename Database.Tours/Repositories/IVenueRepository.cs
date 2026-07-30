using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface IVenueRepository : ISingleKeyRepositoryBase<VenueDo, uint>, IRepositoryBase<VenueDo>
{
}