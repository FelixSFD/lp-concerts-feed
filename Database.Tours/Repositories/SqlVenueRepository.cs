using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlVenueRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<VenueDo, uint>(dbContext, dbContext.Venues), IVenueRepository
{
    protected override async Task<VenueDo> LoadReferences(VenueDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(v => v.Country)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(v => v.State)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(v => v.City)
            .LoadAsync();
        
        return dataObject;
    }
}