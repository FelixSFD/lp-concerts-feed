using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public abstract class SingleKeySqlRepositoryBase<TDataObject, TPrimaryKey> : SqlRepositoryBase<TDataObject>, ISingleKeyRepositoryBase<TDataObject, TPrimaryKey>
    where TDataObject : BaseDo
{
    public SingleKeySqlRepositoryBase(DbContext dbContext, DbSet<TDataObject> dbSet) : base(dbContext, dbSet)
    {
    }

    /// <inheritdoc />
    public virtual async Task<TDataObject?> GetByPrimaryKeyAsync(TPrimaryKey primaryKey)
    {
        var loadedObject = await DbSet.FindAsync(primaryKey);
        if (loadedObject == null)
            return null;
        
        return await LoadReferences(loadedObject);
    }


    /// <summary>
    /// Loads the referenced objects for the <paramref name="dataObject" />
    /// </summary>
    /// <param name="dataObject">The object that was retrieved from the DB, but has no referenced data yet</param>
    /// <returns>the <paramref name="dataObject"/> but with all referenced objects</returns>
    protected abstract Task<TDataObject> LoadReferences(TDataObject dataObject);
}