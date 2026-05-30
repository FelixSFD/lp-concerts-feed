using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlConcertRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<ConcertDo, string>(dbContext, dbContext.Concerts), IConcertRepository
{
    protected override Task<ConcertDo> LoadReferences(ConcertDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}