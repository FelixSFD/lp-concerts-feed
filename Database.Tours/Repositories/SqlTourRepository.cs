using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlTourRepository(ToursDbContext dbContext)
    : SingleKeySqlRepositoryBase<TourDo, string>(dbContext, dbContext.Tours), ITourRepository
{
    protected override Task<TourDo> LoadReferences(TourDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}