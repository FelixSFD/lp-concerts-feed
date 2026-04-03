using Database.Setlists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory to get a DbContext.
/// This is mainly used for creating EF migrations
/// </summary>
public class SetlistsDbContextFactory 
    : IDesignTimeDbContextFactory<SetlistsDbContext>
{
    public SetlistsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SetlistsDbContext>();

        var connectionString = args.FirstOrDefault()
                               ?? throw new ArgumentException("Connection string not provided.");

        optionsBuilder.UseMySQL(connectionString);

        return new SetlistsDbContext(optionsBuilder.Options);
    }
}