using Microsoft.Extensions.Logging;

namespace Service.SharePics;

public static partial class Log
{
    [LoggerMessage(
        EventId = 20001,
        Level = LogLevel.Debug,
        Message = "Sending HTML to Gotenberg for conversion to PNG image:\n{html}"
    )]
    public static partial void SendingHtmlToGotenberg(ILogger logger, string html);

    [LoggerMessage(
        EventId = 20002,
        Level = LogLevel.Information,
        Message = "Gotenberg conversion succeeded"
    )]
    public static partial void GotenbergConversionSucceeded(ILogger logger);

    [LoggerMessage(
        EventId = 20003,
        Level = LogLevel.Debug,
        Message = "Looking for template directory for templateId: {templateId}"
    )]
    public static partial void LookingForTemplateDirectory(ILogger logger, string templateId);
    
    [LoggerMessage(
        EventId = 20004,
        Level = LogLevel.Debug,
        Message = "Found template '{templateId}'  in directory: {templateDir}"
    )]
    public static partial void FoundTemplate(ILogger logger, string templateId, string templateDir);
}