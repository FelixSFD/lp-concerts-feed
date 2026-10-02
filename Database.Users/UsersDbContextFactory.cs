using Database.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory to get a DbContext.
/// This is mainly used for creating EF migrations
/// </summary>
public class UsersDbContextFactory 
    : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();

        var connectionString = args.FirstOrDefault()
                               ?? throw new ArgumentException("Connection string not provided.");

        optionsBuilder.UseMySQL(connectionString);

        return new UsersDbContext(optionsBuilder.Options);
    }
}