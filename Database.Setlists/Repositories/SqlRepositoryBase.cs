using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlRepositoryBase<TDataObject> : IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    public void Save(TDataObject data)
    {
        throw new NotImplementedException();
    }

    public void Delete(TDataObject data)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}