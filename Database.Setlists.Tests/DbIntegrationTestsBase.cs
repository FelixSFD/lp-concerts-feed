using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Tests;

using Testcontainers.MySql;
using Xunit;

public abstract class DbIntegrationTestsBase : IAsyncLifetime
{
    private readonly MySqlContainer _dbContainer;
    protected SetlistsDbContext DbContext;

    public DbIntegrationTestsBase()
    {
        _dbContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        var optBuilder = new DbContextOptionsBuilder<SetlistsDbContext>();
        optBuilder.UseMySQL(_dbContainer.GetConnectionString());
        //optBuilder.UseMySQL("server=lpdb.felixsfd.de;Port=6612;uid=admin;pwd=;database=lpdb");
        DbContext = new SetlistsDbContext(optBuilder.Options);

        await DbContext.Database.EnsureCreatedAsync();
        //await RunDbScriptsAsync();
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