using Database.Users.DataObjects;
using Database.Users.Repositories;
using Microsoft.Extensions.Logging;
using Service.Users.DataStructure;

namespace Service.Users;

/// <summary>
/// Service to manage users
/// </summary>
/// <param name="userRepository"></param>
/// <param name="logger"></param>
public class UserService(IUserRepository userRepository, ILogger<UserService> logger)
{
    /// <summary>
    /// Creates a new user in the database. This does not automatically create the user in AWS Cognito
    /// </summary>
    /// <param name="username">name of the new user</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<UserBo> CreateUserAsync(string username, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating new user with username '{username}'", username);
        var user = new UserDo
        {
            Id = Guid.NewGuid().ToString(),
            Username = username
        };
        
        userRepository.Add(user);
        await userRepository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User created with id '{id}'", user.Id);

        user = await userRepository.GetByPrimaryKeyAsync(user.Id) ?? throw new InvalidOperationException("User not found"); // TODO: Better exception

        return user.ToBo();
    }
}