using Common.Database.MySql.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

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
        await Context.Entry(dataObject)
            .Collection(v => v.PreviousNames)
            .LoadAsync();
        
        return dataObject;
    }

    protected override IQueryable<VenueDo> DefaultQueryConfiguration(IQueryable<VenueDo> queryable)
    {
        return base.DefaultQueryConfiguration(queryable)
            .Include(v => v.Country)
            .Include(v => v.State)
            .Include(v => v.City)
            .Include(v => v.PreviousNames);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<VenueDo> GetChangedAfter(DateTimeOffset timestamp)
    {
        var query = DbSet.AsQueryable();
        query = DefaultQueryConfiguration(query);
        return query
            .Where(v => v.UpdatedAt > timestamp || v.CreatedAt > timestamp)
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<VenueDo> GetDeletedAfter(DateTimeOffset timestamp)
    {
        var query = DbSet.AsQueryable();
        query = DefaultQueryConfiguration(query);
        return query
            .Where(v => v.CreatedAt <= timestamp && v.DeletedAt > timestamp)
            .ToAsyncEnumerable();
    }
}