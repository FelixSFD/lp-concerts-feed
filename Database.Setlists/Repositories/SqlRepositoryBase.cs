using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlRepositoryBase<TDataObject> : IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    protected DbContext Context { get; }
    protected DbSet<TDataObject> DbSet { get; }

    
    public SqlRepositoryBase(DbContext dbContext, DbSet<TDataObject> dbSet)
    {
        Context = dbContext;
        DbSet = dbSet;
    }

    public virtual void Add(TDataObject data)
    {
        DbSet.Add(data);
    }

    public virtual void Delete(TDataObject data)
    {
        DbSet.Remove(data);
    }

    public virtual IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }


    public async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
}