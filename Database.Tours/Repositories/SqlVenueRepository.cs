using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlVenueRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<VenueDo, uint>(dbContext, dbContext.Venues), IVenueRepository
{
    protected override Task<VenueDo> LoadReferences(VenueDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}