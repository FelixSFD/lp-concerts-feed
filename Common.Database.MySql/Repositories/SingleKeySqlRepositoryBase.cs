using Common.Database.DataObjects;
using Common.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.MySql.Repositories;

public abstract class SingleKeySqlRepositoryBase<TDataObject, TPrimaryKey> : SqlRepositoryBase<TDataObject>, ISingleKeyRepositoryBase<TDataObject, TPrimaryKey>
    where TDataObject : BaseDo
{
    public SingleKeySqlRepositoryBase(DbContext dbContext, DbSet<TDataObject> dbSet) : base(dbContext, dbSet)
    {
    }

    /// <inheritdoc />
    public virtual async Task<TDataObject?> GetByPrimaryKeyAsync(TPrimaryKey primaryKey, CancellationToken cancellationToken = default)
    {
        var loadedObject = await GetByPrimaryKeyWithoutReferencesAsync(primaryKey, cancellationToken);
        if (loadedObject == null)
            return null;
        
        return await LoadReferences(loadedObject);
    }

    /// <inheritdoc/>
    public virtual async Task<TDataObject?> GetByPrimaryKeyWithoutReferencesAsync(TPrimaryKey primaryKey, CancellationToken cancellationToken = default)
    {
       return await DbSet.FindAsync([primaryKey], cancellationToken);
    }
}