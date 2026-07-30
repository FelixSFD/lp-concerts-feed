using Database.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory to get a DbContext.
/// This is mainly used for creating EF migrations
/// </summary>
public class ToursDbContextFactory 
    : IDesignTimeDbContextFactory<ToursDbContext>
{
    public ToursDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ToursDbContext>();

        var connectionString = args.FirstOrDefault()
                               ?? throw new ArgumentException("Connection string not provided.");

        optionsBuilder.UseMySQL(connectionString);

        return new ToursDbContext(optionsBuilder.Options);
    }
}