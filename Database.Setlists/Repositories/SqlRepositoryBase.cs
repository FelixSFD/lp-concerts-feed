using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlRepositoryBase<TDataObject>(DbContext dbContext, DbSet<TDataObject> dbSet) : IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    public void Add(TDataObject data)
    {
        dbSet.Add(data);
    }

    public void Delete(TDataObject data)
    {
        dbSet.Remove(data);
    }

    public IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }


    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}