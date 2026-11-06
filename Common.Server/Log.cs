using Microsoft.Extensions.Logging;

namespace Common.Server;

internal static partial class Log
{
    [LoggerMessage(LogLevel.Debug, "Disabled cache for authenticated request.")]
    public static partial void DisabledCacheForAuthenticatedRequest(ILogger? logger);
}