using Common.Database.MySql.Repositories;
using Database.Users.DataObjects;

namespace Database.Users.Repositories;

public class SqlCalendarSubscriptionRepository(UsersDbContext dbContext)
    : SingleKeySqlRepositoryBase<CalendarSubscriptionDo, string>(dbContext, dbContext.CalendarSubscriptions), ICalendarSubscriptionRepository
{
    protected override Task<CalendarSubscriptionDo> LoadReferences(CalendarSubscriptionDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}