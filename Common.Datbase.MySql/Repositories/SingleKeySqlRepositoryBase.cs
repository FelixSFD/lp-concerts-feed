using Common.Database.DataObjects;
using Common.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Common.Datbase.MySql.Repositories;

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

    /// <inheritdoc/>
    public virtual async Task<TDataObject?> GetByPrimaryKeyWithReferencesAsync(TPrimaryKey primaryKey)
    {
        var loadedObject = await GetByPrimaryKeyAsync(primaryKey);
        if (loadedObject == null)
            return null;
        
        return await LoadReferences(loadedObject);
    }
}