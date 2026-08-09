using System.Net;
using Common.Utils;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;

namespace Server.Api.Cache;

/// <summary>
/// Clears the output cache if the associated request finished successfully
/// </summary>
public class ClearCacheAttribute : ResultFilterAttribute
{
    /// <summary>
    /// Tags to clear if the request was successful
    /// </summary>
    public required string[] Tags { get; set; }

    /// <inheritdoc />
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var statusCode = (HttpStatusCode) context.HttpContext.Response.StatusCode;
        if (statusCode.IsSuccessStatusCode() && Tags.Length > 0)
        {
            var outputCacheStore = context.HttpContext.RequestServices.GetRequiredService<IOutputCacheStore>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ClearCacheAttribute>>();
            
            foreach (var tag in Tags)
            {
                logger.LogTrace("Clearing {tag} cache.", tag);
                await outputCacheStore.EvictByTagAsync(tag, context.HttpContext.RequestAborted);
                logger.LogDebug("Evicted {tag} cache.", tag);
            }
        }
    }
}