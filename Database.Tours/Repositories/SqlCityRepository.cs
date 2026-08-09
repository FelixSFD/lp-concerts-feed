using System.Linq.Expressions;
using Common.Database;
using Common.Database.MySql.Repositories;
using Common.Database.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlCityRepository(ToursDbContext dbContext) : SqlRepositoryBase<CityDo>(dbContext, dbContext.Cities), ICityRepository
{
    protected override IReadOnlyDictionary<string, LambdaExpression> SortExpressions { get; } = new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = (Expression<Func<CityDo, string>>)(c => c.Name),
        ["nativeName"] = (Expression<Func<CityDo, string>>)(c => c.NativeName),
        ["countryCode"] = (Expression<Func<CityDo, string>>)(c => c.CountryCode),
        ["country.name"] = (Expression<Func<CityDo, string>>)(c => c.Country.Name),
        ["country.nativeName"] = (Expression<Func<CityDo, string>>)(c => c.Country.NativeName),
        ["state.name"] = (Expression<Func<CityDo, string>>)(c => c.State!.Name),
        ["state.nativeName"] = (Expression<Func<CityDo, string>>)(c => c.State!.NativeName),
    };
    
    protected override async Task<CityDo> LoadReferences(CityDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(v => v.Country)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(v => v.State)
            .LoadAsync();
        
        return dataObject;
    }
    
    public async Task<CityDo?> GetByPrimaryKeyAsync(string countryCode, uint cityId)
    {
        return await DbSet
            .Include(c => c.Country)
            .Include(c => c.State)
            .FirstOrDefaultAsync(c => c.CountryCode == countryCode && c.Id == cityId);
    }

    private static IQueryable<CityDo> IncludeAllReferences(IQueryable<CityDo> queryable) =>
        queryable.Include(c => c.Country)
            .Include(c => c.State);

    public IAsyncEnumerable<CityDo> GetCities(CancellationToken token, string? countryCode = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null)
    {
        paginationParams ??= new PaginationParams(0, 100);
        return FindAsync(c => countryCode == null || c.CountryCode == countryCode, IncludeAllReferences, orderBy, paginationParams);
    }

    public async Task<CityDo?> GetByPrimaryKeyWithoutReferencesAsync(string countryCode, uint cityId)
    {
        return await DbSet.FindAsync(cityId, countryCode);
    }
}