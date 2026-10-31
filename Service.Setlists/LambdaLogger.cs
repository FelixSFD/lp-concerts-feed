using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Service.Setlists;

internal class LambdaLogger(ILogger<LambdaLogger> logger) : ILambdaLogger
{
    public void Log(string message) => logger.LogInformation("{Message}", message);

    public void LogLine(string message) => logger.LogInformation("{Message}", message);

    public void Log(string level, string message, params object[] args)
    {
        var logLevel = level?.ToUpperInvariant() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" => LogLevel.Debug,
            "INFO" or "INFORMATION" => LogLevel.Information,
            "WARN" or "WARNING" => LogLevel.Warning,
            "ERROR" => LogLevel.Error,
            "CRITICAL" or "FATAL" => LogLevel.Critical,
            _ => LogLevel.Information
        };

        logger.Log(logLevel, message, args);
    }
}
