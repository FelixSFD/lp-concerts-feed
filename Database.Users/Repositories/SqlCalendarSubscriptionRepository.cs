using Common.Database.MySql.Repositories;
using Database.Users.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Users.Repositories;

/// <summary>MySQL repository for calendar subscriptions, with ownership checks applied in database queries.</summary>
/// <param name="dbContext">Users database context.</param>
public class SqlCalendarSubscriptionRepository(UsersDbContext dbContext)
    : SingleKeySqlRepositoryBase<CalendarSubscriptionDo, string>(dbContext, dbContext.CalendarSubscriptions), ICalendarSubscriptionRepository
{
    /// <summary>Restricts a query to the specified subscription when it is unowned or owned by the caller.</summary>
    private IQueryable<CalendarSubscriptionDo> Accessible(string id, string? userId)
    {
        var query = dbContext.CalendarSubscriptions.Where(s => s.Id == id);
        return userId is null
            ? query.Where(s => s.UserId == null)
            : query.Where(s => s.UserId == null || EF.Functions.Collate(s.UserId, "utf8mb4_bin") == userId);
    }

    /// <inheritdoc />
    public Task<CalendarSubscriptionDo?> GetAccessibleAsync(string id, string? userId, CancellationToken cancellationToken = default) =>
        Accessible(id, userId).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public Task<CalendarSubscriptionDo[]> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        dbContext.CalendarSubscriptions.AsNoTracking()
            .Where(s => s.UserId != null && EF.Functions.Collate(s.UserId, "utf8mb4_bin") == userId)
            .OrderBy(s => s.CreatedAt).ThenBy(s => s.Id)
            .ToArrayAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<bool> UpdateSettingsAsync(string id, string? userId, bool includeDoorsTime, bool includeLpStageTime, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await Accessible(id, userId).ExecuteUpdateAsync(setters => setters
            .SetProperty(s => s.IncludeDoorsTime, includeDoorsTime)
            .SetProperty(s => s.IncludeLpStageTime, includeLpStageTime)
            .SetProperty(s => s.UpdatedAt, now), cancellationToken) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAccessibleAsync(string id, string? userId, CancellationToken cancellationToken = default) =>
        await Accessible(id, userId).ExecuteDeleteAsync(cancellationToken) > 0;

    /// <inheritdoc />
    public async Task<bool> ClaimAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await dbContext.CalendarSubscriptions.Where(s => s.Id == id && s.UserId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.UserId, userId)
                .SetProperty(s => s.UpdatedAt, now), cancellationToken) > 0;
    }

    /// <inheritdoc />
    protected override Task<CalendarSubscriptionDo> LoadReferences(CalendarSubscriptionDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}