using Common.Database.MySql.Repositories;
using Database.Users.DataObjects;

namespace Database.Users.Repositories;

/// <summary>
/// Repository to manage users in the SQL database
/// </summary>
public class SqlUserRepository(UsersDbContext dbContext)
    : SingleKeySqlRepositoryBase<UserDo, string>(dbContext, dbContext.Users), IUserRepository
{
    /// <inheritdoc/>
    protected override Task<UserDo> LoadReferences(UserDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}