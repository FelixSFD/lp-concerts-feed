using Amazon.Lambda.Core;
using Amazon.SQS;
using Database.Setlists.Repositories;
using Database.Tours.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Service.Setlists.Importer;

namespace Service.Setlists;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="SetlistService"/> and its dependencies.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSetlistsService(this IServiceCollection services)
    {
        services.AddScoped<ISetlistRepository, SqlSetlistRepository>();
        services.AddScoped<ISetlistEntryRepository, SqlSetlistEntryRepository>();
        services.AddScoped<ISetlistActRepository, SqlSetlistActRepository>();
        services.AddScoped<ISongRepository, SqlSongRepository>();
        services.AddScoped<ISongVariantRepository, SqlSongVariantRepository>();
        services.AddScoped<ISongMashupRepository, SqlSongMashupRepository>();
        services.AddScoped<IAlbumRepository, SqlAlbumRepository>();
        services.AddScoped<IConcertRepository, SqlConcertRepository>();
        services.AddAWSService<IAmazonSQS>();
        services.AddScoped<ISetlistPushEventSender, SetlistPushEventSender>();
        services.AddScoped<IWikitextParser, WikitextParser>();
        services.AddScoped<ILambdaLogger, LambdaLogger>();
        services.AddScoped<SetlistService>();
        services.AddScoped<AlbumService>();
        services.AddScoped<SongService>();
        services.AddScoped<LinkinpediaImportService>();
        return services;
    }
}