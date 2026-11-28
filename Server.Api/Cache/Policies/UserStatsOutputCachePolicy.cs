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
    {
        var response = context.HttpContext.Response;

        // By default, only 200 would be cached, but in this case, we also want to cache 201 responses (created resources)
        context.AllowCacheStorage = response.StatusCode is StatusCodes.Status201Created or StatusCodes.Status200OK;
        
        return ValueTask.CompletedTask;
    }
}
