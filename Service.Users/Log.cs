using Microsoft.Extensions.Logging;

namespace Service.Users;

public static partial class Log
{
    [LoggerMessage(LogLevel.Information, "Creating new user with username '{username}'...")]
    public static partial void CreatingNewUserWithUsername(ILogger logger, string username);
    
    [LoggerMessage(LogLevel.Information, "Successfully created user with username '{username}'! (ID: {id})")]
    public static partial void CreatedNewUserWithUsernameAndId(ILogger logger, string username, string id);
}