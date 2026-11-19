namespace Service.Users.DataStructure;

/// <summary>Calendar subscription settings and ownership information.</summary>
/// <param name="Id">Subscription UUID. For an unowned subscription, possession of this ID permits management.</param>
/// <param name="UserId">Owner's user ID, or null when the subscription is unowned.</param>
/// <param name="IncludeDoorsTime">Whether the feed includes the doors opening time.</param>
/// <param name="IncludeLpStageTime">Whether the feed includes the Linkin Park stage time.</param>
/// <param name="LastGeneratedAt">The time when the calendar was last generated. This is used to determine if the calendar needs to be regenerated.</param>
/// <param name="CreatedAt">Time at which the subscription was created.</param>
/// <param name="UpdatedAt">Time of the last modification, or null if never modified.</param>
public sealed record CalendarSubscriptionBo(
    string Id,
    string? UserId,
    bool IncludeDoorsTime,
    bool IncludeLpStageTime,
    DateTimeOffset? LastGeneratedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
