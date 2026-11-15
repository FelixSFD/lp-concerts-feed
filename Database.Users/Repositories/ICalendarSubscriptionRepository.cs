using Common.Database.Repositories;
using Database.Users.DataObjects;

namespace Database.Users.Repositories;

/// <summary>
/// Repository to manage calendar subscriptions
/// </summary>
public interface ICalendarSubscriptionRepository : ISingleKeyRepositoryBase<CalendarSubscriptionDo, string>, IRepositoryBase<CalendarSubscriptionDo>
{
}