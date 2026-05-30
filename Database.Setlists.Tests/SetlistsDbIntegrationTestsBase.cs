using Common.Database.Tests;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Tests;

public class SetlistsDbIntegrationTestsBase : DbIntegrationTestsBase<SetlistsDbContext>
{
    /// <inheritdoc/>
    protected override SetlistsDbContext CreateDbContext(DbContextOptions<SetlistsDbContext> options) => new(options);
}