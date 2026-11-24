using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Database.Tours.Tests;

/// <summary>Verifies the connection used by the shared database integration test base.</summary>
public class DbIntegrationConnectionTest : ToursDbIntegrationTestsBase
{
    [Fact]
    public async Task TestContainerConnectionDoesNotUseTls()
    {
        var settings = new MySqlConnectionStringBuilder(DbContext.Database.GetConnectionString());
        Assert.Equal(MySqlSslMode.Disabled, settings.SslMode);
        Assert.True(settings.AllowPublicKeyRetrieval);

        await DbContext.Database.OpenConnectionAsync();
        try
        {
            await using var command = DbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SHOW SESSION STATUS LIKE 'Ssl_cipher'";
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(string.Empty, reader.GetString(1));
        }
        finally
        {
            await DbContext.Database.CloseConnectionAsync();
        }
    }
}
