using Common.Database.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlCountryRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<CountryDo, string>(dbContext, dbContext.Countries), ICountryRepository
{
    protected override Task<CountryDo> LoadReferences(CountryDo dataObject)
    {
        return Task.FromResult(dataObject);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CountryDo> GetChangedAfter(DateTimeOffset timestamp)
    {
        return DbSet
            .Where(c => c.UpdatedAt > timestamp || c.CreatedAt > timestamp)
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CountryDo> GetDeletedAfter(DateTimeOffset timestamp)
    {
        return DbSet
            .Where(c => c.CreatedAt <= timestamp && c.DeletedAt > timestamp)
            .ToAsyncEnumerable();
    }
}