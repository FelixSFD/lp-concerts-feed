using Common.Database.Tests;
using Microsoft.EntityFrameworkCore;

namespace Database.Users.Tests;

public class UsersDbIntegrationTestsBase : DbIntegrationTestsBase<UsersDbContext>
{
    /// <inheritdoc/>
    protected override UsersDbContext CreateDbContext(DbContextOptions<UsersDbContext> options) => new(options);
}