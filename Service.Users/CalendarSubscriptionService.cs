using Database.Users.DataObjects;
using Database.Users.Repositories;
using Service.Users.DataStructure;
using Service.Users.Exceptions;

namespace Service.Users;

/// <summary>
/// Manages calendar subscriptions. Caller IDs must come from a trusted authenticated identity;
/// null represents an anonymous caller. Unowned subscriptions are accessible by ID, while owned
/// subscriptions are accessible only to their owner.
/// </summary>
/// <param name="subscriptions">Subscription repository.</param>
/// <param name="users">Repository used to verify local user profiles.</param>
public class CalendarSubscriptionService(
    ICalendarSubscriptionRepository subscriptions,
    IUserRepository users)
{
    /// <summary>Creates a subscription with a generated UUID and assigns it to the caller when authenticated.</summary>
    /// <param name="userId">Authenticated caller's ID, or null to create an unowned subscription.</param>
    /// <param name="includeDoorsTime">Whether to include the doors opening time.</param>
    /// <param name="includeLpStageTime">Whether to include the Linkin Park stage time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The created subscription.</returns>
    /// <exception cref="MissingUserProfileException">The authenticated caller has no local user profile.</exception>
    public async Task<CalendarSubscriptionBo> CreateAsync(string? userId, bool includeDoorsTime, bool includeLpStageTime, CancellationToken cancellationToken = default)
    {
        if (userId is not null)
            await RequireProfileAsync(userId, cancellationToken);

        var subscription = new CalendarSubscriptionDo
        {
            UserId = userId,
            IncludeDoorsTime = includeDoorsTime,
            IncludeLpStageTime = includeLpStageTime
        };
        subscriptions.Add(subscription);
        await subscriptions.SaveChangesAsync(cancellationToken);
        return subscription.ToBo();
    }

    /// <summary>Retrieves a subscription accessible to the caller.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID, or null for an anonymous caller.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The subscription, or null if it does not exist or is inaccessible.</returns>
    public async Task<CalendarSubscriptionBo?> GetAsync(string id, string? userId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptions.GetAccessibleAsync(id, userId, cancellationToken);
        return subscription?.ToBo();
    }

    /// <summary>Lists subscriptions owned by the specified user.</summary>
    /// <param name="userId">Owner's user ID.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The user's owned subscriptions, excluding unowned subscriptions.</returns>
    public async Task<CalendarSubscriptionBo[]> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        (await subscriptions.GetForUserAsync(userId, cancellationToken)).Select(DoMapper.ToBo).ToArray();

    /// <summary>Replaces both settings of an accessible subscription.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID, or null for an anonymous caller.</param>
    /// <param name="includeDoorsTime">Whether to include the doors opening time.</param>
    /// <param name="includeLpStageTime">Whether to include the Linkin Park stage time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>True if updated; false if the subscription is missing or inaccessible.</returns>
    public Task<bool> UpdateAsync(string id, string? userId, bool includeDoorsTime, bool includeLpStageTime, CancellationToken cancellationToken = default) =>
        subscriptions.UpdateSettingsAsync(id, userId, includeDoorsTime, includeLpStageTime, cancellationToken);

    /// <summary>Deletes an accessible subscription.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID, or null for an anonymous caller.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>True if deleted; false if the subscription is missing or inaccessible.</returns>
    public Task<bool> DeleteAsync(string id, string? userId, CancellationToken cancellationToken = default) =>
        subscriptions.DeleteAccessibleAsync(id, userId, cancellationToken);

    /// <summary>Claims an unowned subscription for the caller; claiming a subscription already owned by the caller is idempotent.</summary>
    /// <param name="id">Subscription ID.</param>
    /// <param name="userId">Authenticated caller's ID.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The caller's subscription, or null if it is missing or owned by another user.</returns>
    /// <exception cref="MissingUserProfileException">The caller has no local user profile.</exception>
    public async Task<CalendarSubscriptionBo?> ClaimAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        await RequireProfileAsync(userId, cancellationToken);
        await subscriptions.ClaimAsync(id, userId, cancellationToken);
        var subscription = await subscriptions.GetAccessibleAsync(id, userId, cancellationToken);
        return subscription is not null && string.Equals(subscription.UserId, userId, StringComparison.Ordinal)
            ? subscription.ToBo()
            : null;
    }

    /// <summary>Ensures the caller has the local profile required by the subscription's foreign key.</summary>
    private async Task RequireProfileAsync(string userId, CancellationToken cancellationToken)
    {
        if (await users.GetByPrimaryKeyAsync(userId, cancellationToken) is null)
            throw new MissingUserProfileException();
    }
}
