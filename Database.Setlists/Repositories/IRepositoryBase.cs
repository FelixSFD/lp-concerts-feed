using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    public void Save(TDataObject data);
    public void Delete(TDataObject data);
    public IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token);
}