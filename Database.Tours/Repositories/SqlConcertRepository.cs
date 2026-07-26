using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

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
}