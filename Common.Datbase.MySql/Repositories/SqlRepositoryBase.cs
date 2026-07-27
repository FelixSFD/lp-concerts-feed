using System.Linq.Expressions;
using Common.Database.DataObjects;
using Common.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Common.Datbase.MySql.Repositories;

public abstract class SqlRepositoryBase<TDataObject> : IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    protected DbContext Context { get; }
    protected DbSet<TDataObject> DbSet { get; }

    
    public SqlRepositoryBase(DbContext dbContext, DbSet<TDataObject> dbSet)
    {
        Context = dbContext;
        DbSet = dbSet;
    }

    /// <summary>
    /// Loads the referenced objects for the <paramref name="dataObject" />
    /// </summary>
    /// <param name="dataObject">The object that was retrieved from the DB, but has no referenced data yet</param>
    /// <returns>the <paramref name="dataObject"/> but with all referenced objects</returns>
    protected abstract Task<TDataObject> LoadReferences(TDataObject dataObject);

    public virtual void Add(TDataObject data)
    {
        DbSet.Add(data);
    }
    
    public virtual void Update(TDataObject data)
    {
        DbSet.Update(data);
    }

    public virtual void Delete(TDataObject data)
    {
        DbSet.Remove(data);
    }

    [Obsolete("Use FindAsync() instead")]
    public virtual IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token)
    {
        return DbSet.AsAsyncEnumerable();
    }

    /// <summary>
    /// Runs a query for objects in the repository
    /// </summary>
    /// <param name="predicate">Filter for the query</param>
    /// <param name="configureQuery">Optional parameter for further configuration of the query. If referenced objects should be included, do that in this parameter</param>
    /// <param name="paginationParams">Pagination of the results</param>
    /// <returns>The results matching the <paramref name="predicate"/></returns>
    protected IAsyncEnumerable<TDataObject> FindAsync(Expression<Func<TDataObject, bool>> predicate, Func<IQueryable<TDataObject>, IQueryable<TDataObject>>? configureQuery = null, IPaginationParams? paginationParams = null)
    {
        IQueryable<TDataObject> query = DbSet;

        if (configureQuery != null)
            query = configureQuery(query);
        
        return query
            .Where(predicate)
            .ApplyPagination(paginationParams)
            .ToAsyncEnumerable();
    }


    public async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
}