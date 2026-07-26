using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlConcertRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<ConcertDo, string>(dbContext, dbContext.Concerts), IConcertRepository
{
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
        await Context.Entry(dataObject.Venue.State)
            .Reference(vs => vs.Country)
            .LoadAsync();
        
        return dataObject;
    }
    
    public IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, string? countryCode = null)
    {
        return StartQuery(token)
            .Include(c => c.Type)
            .Include(c => c.Venue)
            .Include(c => c.Venue.City)
            .Include(c => c.Venue.Country)
            .Include(c => c.Venue.State)
            .Include(c => c.Tour)
            .Include(c => c.TourLeg)
            .Where(c => countryCode == null || c.Venue.CountryCode == countryCode)
            .ToAsyncEnumerable();
    }
}