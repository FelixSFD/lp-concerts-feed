using System.Linq.Expressions;
using Common.Database.MySql.Repositories;
using Database.Users.DataObjects;

namespace Database.Users.Repositories;

/// <summary>
/// Repository to manage users in the SQL database
/// </summary>
public class SqlUserRepository(UsersDbContext dbContext)
    : SingleKeySqlRepositoryBase<UserDo, string>(dbContext, dbContext.Users), IUserRepository
{
    protected override IReadOnlyDictionary<string, LambdaExpression> SortExpressions { get; } =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = (Expression<Func<UserDo, string>>)(c => c.Id),
            ["username"] = (Expression<Func<UserDo, string>>)(c => c.Username),
            ["createdAt"] = (Expression<Func<UserDo, DateTimeOffset>>)(c => c.CreatedAt),
            ["updatedAt"] = (Expression<Func<UserDo, DateTimeOffset?>>)(c => c.UpdatedAt),
        };

    /// <inheritdoc/>
    protected override Task<UserDo> LoadReferences(UserDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}