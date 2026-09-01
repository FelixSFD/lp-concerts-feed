using System.Linq.Expressions;
using Common.Database;
using Common.Database.Repositories;
using Common.Database.MySql.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlConcertRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<ConcertDo, string>(dbContext, dbContext.Concerts), IConcertRepository
{
    protected override IReadOnlyDictionary<string, LambdaExpression> SortExpressions { get; } = new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
    {
        ["date"] = (Expression<Func<ConcertDo, DateTimeOffset>>)(c => c.PostedStartTime),
        ["venue"] = (Expression<Func<ConcertDo, string>>)(c => c.Venue.CurrentName),
        ["city"] = (Expression<Func<ConcertDo, string>>)(c => c.Venue.City.Name),
        ["country"] = (Expression<Func<ConcertDo, string>>)(c => c.Venue.Country.Name),
        ["tour"] = (Expression<Func<ConcertDo, string>>)(c => c.Tour!.Name),
        ["type"] = (Expression<Func<ConcertDo, string>>)(c => c.Type.Name)
    };
    
    protected override async Task<ConcertDo> LoadReferences(ConcertDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(c => c.Type)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(c => c.Tour)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(c => c.TourLeg)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(c => c.Venue)
            .LoadAsync();
        
        await Context.Entry(dataObject.Venue)
            .Reference(v => v.City)
            .LoadAsync();
        await Context.Entry(dataObject.Venue)
            .Reference(v => v.State)
            .LoadAsync();
        await Context.Entry(dataObject.Venue)
            .Reference(v => v.Country)
            .LoadAsync();
        await Context.Entry(dataObject.Venue.City)
            .Reference(vc => vc.Country)
            .LoadAsync();
        await Context.Entry(dataObject.Venue.City)
            .Reference(vc => vc.State)
            .LoadAsync();

        var venueState = dataObject.Venue.State;
        if (venueState is not null)
        {
            await Context.Entry(venueState)
                .Reference(vs => vs.Country)
                .LoadAsync();
        }
        
        return dataObject;
    }

    private static IQueryable<ConcertDo> IncludeAllReferences(IQueryable<ConcertDo> queryable) =>
        queryable.Include(c => c.Type)
            .Include(c => c.Venue)
            .Include(c => c.Venue.City)
            .Include(c => c.Venue.Country)
            .Include(c => c.Venue.State)
            .Include(c => c.Tour)
            .Include(c => c.TourLeg);

    public IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, string? countryCode = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null, bool includeDeleted = false)
    {
        paginationParams ??= new PaginationParams(0, 100);
        
        var filter = new ConcertFilter
        {
            CountryCode = countryCode,
        };
        return GetConcerts(token, filter, orderBy, paginationParams, includeDeleted);
    }
    
    public IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, ConcertFilter? filter = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null, bool includeDeleted = false)
    {
        paginationParams ??= new PaginationParams(0, 100);
        return FindDeletableAsync(
            c => filter == null || 
                 (filter.CountryCode == null || c.Venue.CountryCode == filter.CountryCode) &&
                 (filter.Before == null || c.PostedStartTime < filter.Before) &&
                 (filter.After == null || c.PostedStartTime > filter.After),
            IncludeAllReferences, orderBy, paginationParams, includeDeleted);
    }
}