namespace Service.Tours.DataStructure.Stats;

/// <summary>
/// Restricts statistics to a user's attended concerts, a tour, both, or neither.
/// User scopes retain the existing definition of attendance, including its past-concert cutoff.
/// All scopes exclude deleted concerts; global scopes also include future concerts.
/// </summary>
public sealed record StatsScope
{
    /// <summary>Creates a scope; null means that the corresponding filter is not applied.</summary>
    public StatsScope(string? userId = null, string? tourId = null)
    {
        if (userId is not null && string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID must not be empty.", nameof(userId));
        if (tourId is not null && string.IsNullOrWhiteSpace(tourId))
            throw new ArgumentException("Tour ID must not be empty.", nameof(tourId));

        UserId = userId;
        TourId = tourId;
    }

    /// <summary>The user whose attended concerts are selected, or null for all concerts.</summary>
    public string? UserId { get; }

    /// <summary>The selected tour, or null for all tours and concerts without a tour.</summary>
    public string? TourId { get; }
}

