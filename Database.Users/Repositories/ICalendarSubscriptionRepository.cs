using Common.Database.Repositories;
using Database.Users.DataObjects;

namespace Database.Users.Repositories;

/// <summary>
/// Persists calendar subscriptions and enforces ownership on access and mutations.
/// An unowned subscription is accessible to anyone who knows its ID; an owned subscription
/// is accessible only when the supplied user ID matches its owner.
/// </summary>
public interface ICalendarSubscriptionRepository : ISingleKeyRepositoryBase<CalendarSubscriptionDo, string>, IRepositoryBase<CalendarSubscriptionDo>
{
    /// <summary>Finds a subscription accessible to the caller.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID, or null for an anonymous caller.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The subscription, or null if it does not exist or is inaccessible.</returns>
    Task<CalendarSubscriptionDo?> GetAccessibleAsync(string id, string? userId, CancellationToken cancellationToken = default);

    /// <summary>Lists subscriptions owned by the specified user, ordered by creation time and ID.</summary>
    /// <param name="userId">Owner's user ID.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The user's owned subscriptions; unowned subscriptions are excluded.</returns>
    Task<CalendarSubscriptionDo[]> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Atomically replaces the settings of an accessible subscription and updates its modification time.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID, or null for an anonymous caller.</param>
    /// <param name="includeDoorsTime">Whether to include the doors opening time.</param>
    /// <param name="includeLpStageTime">Whether to include the Linkin Park stage time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>True if the subscription was updated; otherwise false.</returns>
    Task<bool> UpdateSettingsAsync(string id, string? userId, bool includeDoorsTime, bool includeLpStageTime, CancellationToken cancellationToken = default);

    /// <summary>Atomically deletes a subscription if it is accessible to the caller.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID, or null for an anonymous caller.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>True if the subscription was deleted; otherwise false.</returns>
    Task<bool> DeleteAccessibleAsync(string id, string? userId, CancellationToken cancellationToken = default);

    /// <summary>Atomically assigns an unowned subscription to a user and updates its modification time.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">ID of an existing user to assign as owner.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>True if ownership was assigned; false if the subscription is missing or already owned.</returns>
    Task<bool> ClaimAsync(string id, string userId, CancellationToken cancellationToken = default);
}