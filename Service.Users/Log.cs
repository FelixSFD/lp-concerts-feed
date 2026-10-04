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
    
    [LoggerMessage(LogLevel.Debug, "Fetching User with ID '{id}'...")]
    public static partial void GetUserByIdStart(ILogger logger, string id);
    
    [LoggerMessage(LogLevel.Debug, "Successfully fetched User with ID '{id}'. Username: {username}")]
    public static partial void GetUserByIdSuccess(ILogger logger, string id, string username);
    
    [LoggerMessage(LogLevel.Information, "Updating User with ID '{id}'...")]
    public static partial void UpdateUserStart(ILogger logger, string id);
    
    [LoggerMessage(LogLevel.Information, "Successfully updated User with ID '{id}'. Username: {username}")]
    public static partial void UpdateUserSuccess(ILogger logger, string id, string username);
    
    [LoggerMessage(LogLevel.Information, "User with ID '{id}' doesn't have a profile yet. Will create a new user")]
    public static partial void RequestedUserNotFoundWillCreate(ILogger logger, string id);
    
    [LoggerMessage(LogLevel.Debug, "Start Fetching paginated users. Skip: '{skip}', Limit: '{limit}'")]
    public static partial void FetchUsersPaginatedStart(ILogger logger, uint skip, uint limit);
    
    [LoggerMessage(LogLevel.Debug, "Successfully fetched paginated users. Skip: '{skip}', Limit: '{limit}', Total Results: '{totalResults}'")]
    public static partial void FetchUsersPaginatedSuccess(ILogger logger, uint skip, uint limit, int totalResults);
}