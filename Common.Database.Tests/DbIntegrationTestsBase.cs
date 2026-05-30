using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Common.Database.Tests;

public abstract class DbIntegrationTestsBase<TTestedDbContext> : IAsyncLifetime where TTestedDbContext : DbContext
{
    private readonly MySqlContainer _dbContainer;
    protected TTestedDbContext DbContext;

    public DbIntegrationTestsBase()
    {
        _dbContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .Build();
    }
    
    /// <summary>
    /// Creates and initializes the <typeparamref name="TTestedDbContext"/>
    /// </summary>
    /// <param name="options">Options for the context</param>
    /// <returns>the created context</returns>
    protected abstract TTestedDbContext CreateDbContext(DbContextOptions<TTestedDbContext> options);

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        var optBuilder = new DbContextOptionsBuilder<TTestedDbContext>();
        optBuilder.UseMySQL(_dbContainer.GetConnectionString());
        DbContext = CreateDbContext(optBuilder.Options);

        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }

    private async Task RunDbScriptsAsync()
    {
        var structureScript = await File.ReadAllTextAsync("DbScripts/0001_InitDb.sql");
        await DbContext.Database.ExecuteSqlRawAsync(structureScript);
    }
}