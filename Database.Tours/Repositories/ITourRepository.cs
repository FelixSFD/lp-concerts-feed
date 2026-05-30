using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

/// <summary>
/// Repository to manage tours
/// </summary>
public interface ITourRepository : ISingleKeyRepositoryBase<TourDo, string>
{
}