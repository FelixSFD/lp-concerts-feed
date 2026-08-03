using Common.Utils.Cache;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Api.Cache;

/// <summary>
/// Enable caching of the response in a shared cache, but not on the client itself
/// </summary>
public sealed class CustomResponseCacheAttribute : ResultFilterAttribute
{
    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int Duration { get; set; } = CacheExpiration.Default;

    /// <summary>
    /// Flags to configure the cache
    /// </summary>
    public CacheFlags CacheFlags { get; set; } = CacheControlHeaderFactory.DefaultCacheFlags;

    /// <inheritdoc />
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        context.HttpContext.Response.Headers.CacheControl = CacheControlHeaderFactory.CacheFor(Duration, CacheFlags);
    }
}