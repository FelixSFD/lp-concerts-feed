using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface IStateRepository : IRepositoryBase<StateDo>
{
    /// <summary>
    /// Returns a state by its primary key
    /// </summary>
    /// <param name="countryCode">ISO code of the country this state is a part of</param>
    /// <param name="stateCode">Code for this state</param>
    /// <returns></returns>
    public Task<StateDo?> GetByPrimaryKeyAsync(string countryCode, string stateCode);
}