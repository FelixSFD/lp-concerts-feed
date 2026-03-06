using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISingleKeyRepositoryBase<TDataObject, TPrimaryKey> where TDataObject : BaseDo
{
    /// <summary>
    /// Returns the <typeparamref name="TDataObject" /> by its primary key of type <typeparamref name="TPrimaryKey"/>
    /// </summary>
    /// <param name="primaryKey">Primary Key</param>
    /// <returns>Found entry or null if it was not found</returns>
    public Task<TDataObject?> GetByPrimaryKeyAsync(TPrimaryKey primaryKey);
}