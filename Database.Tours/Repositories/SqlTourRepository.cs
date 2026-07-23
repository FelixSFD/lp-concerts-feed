using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlTourRepository(ToursDbContext dbContext)
    : SingleKeySqlRepositoryBase<TourDo, string>(dbContext, dbContext.Tours), ITourRepository
{
    protected override async Task<TourDo> LoadReferences(TourDo dataObject)
    {
        await Context.Entry(dataObject)
            .Collection(t => t.Legs)
            .LoadAsync();
        
        return dataObject;
    }
}