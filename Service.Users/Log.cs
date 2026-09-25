using Microsoft.Extensions.Logging;

namespace Service.Users;

public static partial class Log
{
    [LoggerMessage(LogLevel.Information, "Creating new user with username '{username}'...")]
    public static partial void CreatingNewUserWithUsername(ILogger logger, string username);
    
    [LoggerMessage(LogLevel.Information, "Successfully created user with username '{username}'! (ID: {id})")]
    public static partial void CreatedNewUserWithUsernameAndId(ILogger logger, string username, string id);
    
    [LoggerMessage(LogLevel.Warning, "User with ID '{id}' was not found")]
    public static partial void UserNotFound(ILogger logger, string id);
    
    [LoggerMessage(LogLevel.Warning, "Fetching User with ID '{id}'...")]
    public static partial void GetUserByIdStart(ILogger logger, string id);
    
    [LoggerMessage(LogLevel.Warning, "Successfully fetched User with ID '{id}'. Username: {username}")]
    public static partial void GetUserByIdSuccess(ILogger logger, string id, string username);
}