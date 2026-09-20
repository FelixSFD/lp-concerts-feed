using System.Linq.Expressions;
using Common.Database;
using Common.Database.DataObjects;
using Common.Database.Filter;
using Common.Database.Pagination;
using Common.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.MySql.Repositories;

public abstract class SqlRepositoryBase<TDataObject> : IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    protected DbContext Context { get; }
    protected DbSet<TDataObject> DbSet { get; }
    
    /// <summary>
    /// Defines the expressions for the sorting and assigns them to a string value.
    /// That way, we can dynamically pass sorting parameters without having to access the data object in other layers.
    /// </summary>
    protected virtual IReadOnlyDictionary<string, LambdaExpression> SortExpressions =>
        new Dictionary<string, LambdaExpression>();

    
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
    protected IAsyncEnumerable<TDataObject> FindAsync(Expression<Func<TDataObject, bool>> predicate, Func<IQueryable<TDataObject>, IQueryable<TDataObject>>? configureQuery = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null)
    {
        IQueryable<TDataObject> query = DbSet;
        
        orderBy ??= new List<SortDescriptor>();

        if (configureQuery != null)
            query = configureQuery(query);
        
        return query
            .Where(predicate)
            .ApplySorting(orderBy, SortExpressions)
            .ApplyPagination(paginationParams)
            .ToAsyncEnumerable();
    }
    
    /// <summary>
    /// Runs a query for objects in the repository. By using the <paramref name="includeDeleted"/> parameter, deleted objects can be returned, too.
    /// </summary>
    /// <param name="predicate">Filter for the query</param>
    /// <param name="configureQuery">Optional parameter for further configuration of the query. If referenced objects should be included, do that in this parameter</param>
    /// <param name="paginationParams">Pagination of the results</param>
    /// <param name="includeDeleted">true, if entries that are marked as deleted should be returned anyway</param>
    /// <returns>The results matching the <paramref name="predicate"/></returns>
    protected IAsyncEnumerable<TDataObject> FindDeletableAsync(Expression<Func<TDataObject, bool>> predicate, Func<IQueryable<TDataObject>, IQueryable<TDataObject>>? configureQuery = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null, bool includeDeleted = false)
    {
        if (!typeof(TDataObject).IsAssignableTo(typeof(IDeletableDataObject)))
        {
            throw new InvalidCastException($"The data object '{typeof(TDataObject).FullName}' must be of type IDeletableDataObject!");
        }
        
        IQueryable<TDataObject> query = DbSet;
        
        orderBy ??= new List<SortDescriptor>();

        if (configureQuery != null)
            query = configureQuery(query);
        
        if (!includeDeleted)
        {
            query = query
                .Cast<IDeletableDataObject>()
                .NotDeleted()
                .Cast<TDataObject>();
        }
        
        return query
            .Where(predicate)
            .ApplySorting(orderBy, SortExpressions)
            .ApplyPagination(paginationParams)
            .ToAsyncEnumerable();
    }

    protected async Task<PaginatedQueryResult<TDataObject>> FindPaginatedAsync(Expression<Func<TDataObject, bool>> predicate, Func<IQueryable<TDataObject>, IQueryable<TDataObject>>? configureQuery = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (!typeof(TDataObject).IsAssignableTo(typeof(IDeletableDataObject)))
        {
            throw new InvalidCastException($"The data object '{typeof(TDataObject).FullName}' must be of type IDeletableDataObject!");
        }
        
        IQueryable<TDataObject> query = DbSet;
        
        orderBy ??= new List<SortDescriptor>();

        if (configureQuery != null)
            query = configureQuery(query);
        
        if (!includeDeleted)
        {
            query = query
                .Cast<IDeletableDataObject>()
                .NotDeleted()
                .Cast<TDataObject>();
        }

        query = query.Where(predicate);
        
        return await query.ToPaginatedResultAsync(orderBy, SortExpressions, paginationParams, cancellationToken);
    }
    
    /// <summary>
    /// Finds a list of data objects in the database. Pagination is applied, but no metadata will be returned.
    /// <p>
    /// In case you don't need to return metadata of the pagination (like total count), prefer this method over <see cref="FindAsync"/>.
    /// </p>
    /// </summary>
    /// <param name="filter">Filter to use for the query</param>
    /// <param name="configureQuery">additional configuration for the query (like loading references)</param>
    /// <param name="orderBy">Sort order for the query</param>
    /// <param name="paginationParams">Pagination parameters for the query</param>
    /// <param name="includeDeleted">Whether to include deleted objects in the query</param>
    /// <param name="cancellationToken">Cancellation token for the query</param>
    /// <returns>The results with pagination metadata</returns>
    /// <exception cref="InvalidCastException"></exception>
    protected async Task<PaginatedQueryResult<TDataObject>> FindPaginatedAsync(IQueryFilter<TDataObject>? filter = null, Func<IQueryable<TDataObject>, IQueryable<TDataObject>>? configureQuery = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (!typeof(TDataObject).IsAssignableTo(typeof(IDeletableDataObject)))
        {
            throw new InvalidCastException($"The data object '{typeof(TDataObject).FullName}' must be of type IDeletableDataObject!");
        }
        
        IQueryable<TDataObject> query = DbSet;
        
        orderBy ??= new List<SortDescriptor>();

        if (configureQuery != null)
            query = configureQuery(query);
        
        if (!includeDeleted)
        {
            query = query
                .Cast<IDeletableDataObject>()
                .NotDeleted()
                .Cast<TDataObject>();
        }

        if (filter != null)
            query = query.ApplyFilter(filter);
        
        return await query.ToPaginatedResultAsync(orderBy, SortExpressions, paginationParams, cancellationToken);
    }
    
    /// <summary>
    /// Finds a list of data objects in the database. Pagination is applied, but no metadata will be returned.
    /// <p>
    /// Prefer this method over <see cref="FindPaginatedAsync(System.Linq.Expressions.Expression{System.Func{TDataObject,bool}},System.Func{System.Linq.IQueryable{TDataObject},System.Linq.IQueryable{TDataObject}}?,System.Collections.Generic.IEnumerable{Common.Database.SortDescriptor}?,Common.Database.Repositories.IPaginationParams?,bool,System.Threading.CancellationToken)"/> when you don't need to return metadata.
    /// </p>
    /// </summary>
    /// <param name="filter">Filter to use for the query</param>
    /// <param name="configureQuery">additional configuration for the query (like loading references)</param>
    /// <param name="orderBy">Sort order for the query</param>
    /// <param name="paginationParams">Pagination parameters for the query</param>
    /// <param name="includeDeleted">Whether to include deleted objects in the query</param>
    /// <param name="cancellationToken">Cancellation token for the query</param>
    /// <returns>The results without pagination metadata</returns>
    /// <exception cref="InvalidCastException"></exception>
    protected IAsyncEnumerable<TDataObject> FindAsync(IQueryFilter<TDataObject>? filter = null, Func<IQueryable<TDataObject>, IQueryable<TDataObject>>? configureQuery = null, IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (!typeof(TDataObject).IsAssignableTo(typeof(IDeletableDataObject)))
        {
            throw new InvalidCastException($"The data object '{typeof(TDataObject).FullName}' must be of type IDeletableDataObject!");
        }
        
        IQueryable<TDataObject> query = DbSet;
        
        orderBy ??= new List<SortDescriptor>();

        if (configureQuery != null)
            query = configureQuery(query);
        
        if (!includeDeleted)
        {
            query = query
                .Cast<IDeletableDataObject>()
                .NotDeleted()
                .Cast<TDataObject>();
        }

        if (filter != null)
            query = query.ApplyFilter(filter);
        
        return query.ApplyPagination(orderBy, SortExpressions, paginationParams);
    }


    public async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
}