using Amazon.Lambda.Core;
using Amazon.SQS;
using Database.Setlists.Repositories;
using Database.Tours.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Service.Setlists.Importer;

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
        Assert.Contains(services, d => d.ServiceType == typeof(IAlbumRepository) && d.ImplementationType == typeof(SqlAlbumRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(IConcertRepository) && d.ImplementationType == typeof(SqlConcertRepository));
        Assert.Contains(services, d => d.ServiceType == typeof(IAmazonSQS));
        Assert.Contains(services, d => d.ServiceType == typeof(ISetlistPushEventSender) && d.ImplementationType == typeof(SetlistPushEventSender));
        Assert.Contains(services, d => d.ServiceType == typeof(IWikitextParser) && d.ImplementationType == typeof(WikitextParser));
        Assert.Contains(services, d => d.ServiceType == typeof(ILambdaLogger));
        Assert.Contains(services, d => d.ServiceType == typeof(SetlistService) && d.ImplementationType == typeof(SetlistService));
        Assert.Contains(services, d => d.ServiceType == typeof(AlbumService) && d.ImplementationType == typeof(AlbumService));
        Assert.Contains(services, d => d.ServiceType == typeof(SongService) && d.ImplementationType == typeof(SongService));
        Assert.Contains(services, d => d.ServiceType == typeof(LinkinpediaImportService) && d.ImplementationType == typeof(LinkinpediaImportService));
    }
}
