using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;

namespace Service.Setlists;

internal class LambdaLogger(ILogger<LambdaLogger> logger) : ILambdaLogger
{
    public void Log(string message)
    {
        logger.LogInformation("{Message}", message);
    }

    public void LogLine(string message)
    {
        logger.LogInformation("{Message}", message);
    }
}
