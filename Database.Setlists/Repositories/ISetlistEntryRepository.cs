using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public interface ISetlistEntryRepository : ISingleKeyRepositoryBase<SetlistEntryDo, string>, IRepositoryBase<SetlistEntryDo>
{
}