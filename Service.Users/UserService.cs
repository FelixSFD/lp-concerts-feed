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
    /// <param name="id">ID of the new user. If null, a new GUID will be generated</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<UserBo> CreateUserAsync(string username, string? id = null, CancellationToken cancellationToken = default)
    {
        Log.CreatingNewUserWithUsername(logger, username);
        id ??= Guid.NewGuid().ToString();
        var user = new UserDo
        {
            Id = id,
            Username = username
        };
        
        userRepository.Add(user);
        await userRepository.SaveChangesAsync(cancellationToken);
        Log.CreatedNewUserWithUsernameAndId(logger, user.Username, user.Id);

        user = await userRepository.GetByPrimaryKeyAsync(user.Id, cancellationToken) ?? throw new UserNotFoundException(id);

        return user.ToBo();
    }
    
    /// <summary>
    /// Get the information about a user by their ID
    /// </summary>
    /// <param name="id">ID of the user to find</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UserNotFoundException">if the user with the given ID was not found</exception>
    public async Task<UserBo> GetUserById(string id, CancellationToken cancellationToken = default)
    {
        Log.GetUserByIdStart(logger, id);
        var user = await userRepository.GetByPrimaryKeyAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);
        Log.GetUserByIdSuccess(logger, id, user.Username ?? "no username");
        return user.ToBo();
    }

}