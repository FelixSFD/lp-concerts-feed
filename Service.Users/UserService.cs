using Database.Users.DataObjects;
using Database.Users.Repositories;
using Microsoft.Extensions.Logging;
using Service.Users.DataStructure;
using Service.Users.Exceptions;

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
        Log.CreatingNewUserWithUsername(logger, username);
        var id = Guid.NewGuid().ToString();
        var user = new UserDo
        {
            Id = id,
            Username = username
        };
        
        userRepository.Add(user);
        await userRepository.SaveChangesAsync(cancellationToken);
        Log.CreatedNewUserWithUsernameAndId(logger, user.Username, user.Id);

        user = await userRepository.GetByPrimaryKeyAsync(user.Id) ?? throw new UserNotFoundException(id);

        return user.ToBo();
    }
}