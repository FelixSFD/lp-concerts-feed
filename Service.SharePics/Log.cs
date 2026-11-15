using Microsoft.Extensions.Logging;

namespace Service.SharePics;

public static partial class Log
{
    [LoggerMessage(
        EventId = 20001,
        Level = LogLevel.Trace,
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
    
    [LoggerMessage(
        EventId = 20005,
        Level = LogLevel.Debug,
        Message = "Adding additional file to Gotenberg request: {fileName} ({fileSize} bytes)"
    )]
    public static partial void GotenbergAddAdditionalFile(ILogger logger, string fileName, int fileSize);
    
    [LoggerMessage(
        EventId = 20080,
        Level = LogLevel.Debug,
        Message = "Storing object '{objectKey}' permanently in bucket '{bucketName}'..."
    )]
    public static partial void StoringObjectPermanently(ILogger logger, string objectKey, string bucketName);
    
    [LoggerMessage(
        EventId = 20081,
        Level = LogLevel.Debug,
        Message = "Stored object '{objectKey}' permanently in bucket '{bucketName}'."
    )]
    public static partial void StoredObjectPermanently(ILogger logger, string objectKey, string bucketName);
    
    [LoggerMessage(
        EventId = 20082,
        Level = LogLevel.Debug,
        Message = "Storing object '{objectKey}' temporarily in bucket '{bucketName}'..."
    )]
    public static partial void StoringObjectTemporarily(ILogger logger, string objectKey, string bucketName);
    
    [LoggerMessage(
        EventId = 20083,
        Level = LogLevel.Debug,
        Message = "Stored object '{objectKey}' temporarily in bucket '{bucketName}'."
    )]
    public static partial void StoredObjectTemporarily(ILogger logger, string objectKey, string bucketName);
    
    [LoggerMessage(
        EventId = 20084,
        Level = LogLevel.Debug,
        Message = "Added prefix to object key. Full key is now: {objectKey}"
    )]
    public static partial void AddedPrefixToObjectKey(ILogger logger, string objectKey);
}