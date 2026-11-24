using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Testcontainers.MySql;

namespace Common.Database.Tests;

public abstract class DbIntegrationTestsBase<TTestedDbContext> : IAsyncLifetime where TTestedDbContext : DbContext
{
    private readonly MySqlContainer _dbContainer = new MySqlBuilder("mysql:8.0")
        .Build();
    
    protected TTestedDbContext DbContext;

    /// <summary>
    /// Creates and initializes the <typeparamref name="TTestedDbContext"/>
    /// </summary>
    /// <param name="options">Options for the context</param>
    /// <returns>the created context</returns>
    protected abstract TTestedDbContext CreateDbContext(DbContextOptions<TTestedDbContext> options);

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Test containers are isolated and disposable; avoid the driver's non-thread-safe TLS cache.
        var connectionString = new MySqlConnectionStringBuilder(_dbContainer.GetConnectionString())
        {
            SslMode = MySqlSslMode.Disabled,
            // MySQL 8 may require RSA key retrieval for caching_sha2_password without TLS.
            AllowPublicKeyRetrieval = true
        };
        var optBuilder = new DbContextOptionsBuilder<TTestedDbContext>();
        optBuilder.UseMySQL(connectionString.ConnectionString);
        DbContext = CreateDbContext(optBuilder.Options);

        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null) 
            await DbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}