using Database.Setlists.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Setlists.Tests;

public class ServiceCollectionExtensionsTest
{
    [Fact]
    public void AddSetlistsService_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddSetlistsService();

        Assert.Contains(services, d => d.ServiceType == typeof(ISetlistRepository) && d.ImplementationType == typeof(SqlSetlistRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(ISetlistEntryRepository) && d.ImplementationType == typeof(SqlSetlistEntryRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(ISetlistActRepository) && d.ImplementationType == typeof(SqlSetlistActRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(ISongRepository) && d.ImplementationType == typeof(SqlSongRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(ISongVariantRepository) && d.ImplementationType == typeof(SqlSongVariantRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(ISongMashupRepository) && d.ImplementationType == typeof(SqlSongMashupRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(SetlistService) && d.ImplementationType == typeof(SetlistService));
    }
}
