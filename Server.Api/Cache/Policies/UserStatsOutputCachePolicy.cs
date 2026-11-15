using Microsoft.AspNetCore.OutputCaching;

namespace Server.Api.Cache.Policies;

/// <summary>
/// Cache-Tag policy for dynamic user cache. The cache tag includes the ID of a user
/// </summary>
internal sealed class UserStatsOutputCachePolicy(ILogger<UserStatsOutputCachePolicy> logger) : IOutputCachePolicy
{
    /// <inheritdoc />
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.HttpContext.Request.RouteValues["userId"]?.ToString();

        if (string.IsNullOrEmpty(userId))
            return ValueTask.CompletedTask;
        
        context.Tags.Add($"user-{userId}-stats");
        Log.AddedOutputCacheTagForUser(logger, userId);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
