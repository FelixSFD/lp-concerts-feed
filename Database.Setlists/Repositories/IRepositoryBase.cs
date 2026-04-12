using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    public void Add(TDataObject data);
    public void Update(TDataObject data);
    public void Delete(TDataObject data);
    public IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token);
    
    public Task SaveChangesAsync();
}