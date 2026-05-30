using Common.Database.Tests;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Tests;

public class ToursDbIntegrationTestsBase : DbIntegrationTestsBase<ToursDbContext>
{
    /// <inheritdoc/>
    protected override ToursDbContext CreateDbContext(DbContextOptions<ToursDbContext> options) => new(options);
}