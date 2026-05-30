using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlConcertTypeRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<ConcertTypeDo, uint>(dbContext, dbContext.ConcertTypes), IConcertTypeRepository
{
    protected override Task<ConcertTypeDo> LoadReferences(ConcertTypeDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}