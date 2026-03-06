using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public abstract class SingleKeySqlRepositoryBase<TDataObject, TPrimaryKey> : SqlRepositoryBase<TDataObject>, ISingleKeyRepositoryBase<TDataObject, TPrimaryKey>
    where TDataObject : BaseDo
{
    public SingleKeySqlRepositoryBase(DbContext dbContext, DbSet<TDataObject> dbSet) : base(dbContext, dbSet)
    {
    }

    public async Task<TDataObject?> GetByPrimaryKeyAsync(TPrimaryKey primaryKey)
    {
        return await DbSet.FindAsync(primaryKey);
    }
}