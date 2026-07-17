namespace Common.Database.Repositories;

public interface ISingleKeyRepositoryBase<TDataObject, TPrimaryKey>
{
    /// <summary>
    /// Returns the <typeparamref name="TDataObject" /> by its primary key of type <typeparamref name="TPrimaryKey"/>
    /// and loads foreign key references
    /// </summary>
    /// <param name="primaryKey">Primary Key</param>
    /// <returns>Found entry or null if it was not found</returns>
    public Task<TDataObject?> GetByPrimaryKeyAsync(TPrimaryKey primaryKey);
    
    /// <summary>
    /// Returns the <typeparamref name="TDataObject" /> by its primary key of type <typeparamref name="TPrimaryKey"/>.
    /// To load the references, use <see cref="GetByPrimaryKeyAsync"/> instead.
    /// </summary>
    /// <param name="primaryKey">Primary Key</param>
    /// <returns>Found entry or null if it was not found</returns>
    public Task<TDataObject?> GetByPrimaryKeyWithoutReferencesAsync(TPrimaryKey primaryKey);
}