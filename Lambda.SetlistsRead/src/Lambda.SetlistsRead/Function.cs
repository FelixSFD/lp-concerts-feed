using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Common.Utils.Cache;
using Common.Utils.Cors;
using Database.Concerts;
using Database.Setlists;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;
using LPCalendar.DataStructure.Setlists;
using Microsoft.EntityFrameworkCore;
using Service.Setlists;
using Service.Setlists.Exceptions;
using static Lambda.Common.ApiGateway.ApiGatewayResponseHelper;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.SetlistsRead;

public class Function
{
    private IConcertRepository _concertRepository;
    private ISetlistRepository _setlistRepository;
    private ISetlistActRepository _setlistActRepository;
    private ISetlistEntryRepository _setlistEntryRepository;
    private IAlbumRepository _albumRepository;
    private ISongRepository _songRepository;
    private ISongVariantRepository _songVariantRepository;
    private ISongMashupRepository _songMashupRepository;
    private SetlistService _setlistService;
    private SongService _songService;
    private AlbumService _albumService;

    public Function()
    {
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        _concertRepository = DynamoDbConcertRepository.CreateDefault(context.Logger);
        var connectionString = Environment.GetEnvironmentVariable("SETLISTS_DB_CONNECTION_STRING") ?? throw new Exception("Missing environment variable 'SETLISTS_DB_CONNECTION_STRING'!");
        var stopwatch = Stopwatch.StartNew();
        var optBuilder = new DbContextOptionsBuilder<SetlistsDbContext>();
        optBuilder.UseMySQL(connectionString);
        var dbContext = new SetlistsDbContext(optBuilder.Options);
        stopwatch.Stop();
        context.Logger.LogDebug("Init duration of EFCore context: {duration}", stopwatch.ElapsedMilliseconds);
        _setlistRepository = new SqlSetlistRepository(dbContext);
        _setlistActRepository = new SqlSetlistActRepository(dbContext);
        _setlistEntryRepository = new SqlSetlistEntryRepository(dbContext);
        _albumRepository = new SqlAlbumRepository(dbContext);
        _songRepository = new SqlSongRepository(dbContext);
        _songVariantRepository = new SqlSongVariantRepository(dbContext);
        _songMashupRepository = new SqlSongMashupRepository(dbContext);
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository, _songVariantRepository, _songMashupRepository, _setlistActRepository, context.Logger);
        _albumService = new AlbumService(_albumRepository, context.Logger);
        _songService = new SongService(_songRepository, _songVariantRepository, _songMashupRepository, context.Logger);
        
        context.Logger.LogInformation("Called {method} {path}", request.HttpMethod, request.Resource);

        request.PathParameters ??= new Dictionary<string, string>();
        
        var hasSetlistIdPathParameter = request.PathParameters.TryGetValue("setlistId", out var setlistIdStr);
        uint? setlistId = hasSetlistIdPathParameter ? uint.Parse(setlistIdStr!) : null;
        
        var hasSetlistEntryIdPathParameter = request.PathParameters.TryGetValue("setlistEntryId", out var setlistEntryId);
        
        var hasSongIdPathParameter = request.PathParameters.TryGetValue("songId", out var songIdStr);
        uint? songId = hasSongIdPathParameter ? uint.Parse(songIdStr!) : null;
        
        var hasConcertIdPathParameter = request.PathParameters.TryGetValue("id", out var concertId);
        
        if (request is { HttpMethod: "GET", Resource: "/concerts/{id}/setlists" } && hasConcertIdPathParameter && !string.IsNullOrEmpty(concertId))
        {
            // at some point, this might be cached in DynamoDB. In that case, it would be faster to use a different lambda with AOT and no EFcore
            context.Logger.LogInformation("Reading setlists for concert with ID: {concertId}");
            return await HandleGetSetlistsForConcert(concertId, context);
        }

        if (request is { HttpMethod: "GET", Resource: "/setlists/{setlistId}" } && hasSetlistIdPathParameter)
        {
            context.Logger.LogInformation("Reading a setlist...");
            return await HandleGetSetlist(setlistId ?? 0, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/setlists/{setlistId}/entries/{setlistEntryId}" } && hasSetlistIdPathParameter && hasSetlistEntryIdPathParameter)
        {
            context.Logger.LogInformation("Reading setlist entry: {setlistEntryId}", setlistEntryId);
            return await HandleGetSetlistEntry(setlistId ?? 0, setlistEntryId!, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/setlists/{setlistId}/acts" } && hasSetlistIdPathParameter)
        {
            context.Logger.LogInformation("Reading the acts of a setlist...");
            return await HandleGetActsInSetlist(setlistId ?? 0, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/setlists" })
        {
            context.Logger.LogInformation("Reading all setlists...");
            return await HandleGetSetlistHeaders(context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/songs/{songId}" } && hasSongIdPathParameter)
        {
            context.Logger.LogInformation("Requested song with ID: {songId}", songId);
            return await HandleGetSong(songId ?? 0, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/songs" })
        {
            context.Logger.LogInformation("Requested all songs");
            return await HandleGetAllSongs(context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/songs/{songId}/variants" } && hasSongIdPathParameter)
        {
            context.Logger.LogInformation("Requested variants song with ID: {songId}", songId);
            return await HandleGetVariantsOfSong(songId ?? 0, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/mashups" })
        {
            context.Logger.LogInformation("Get all mashups...");
            return await HandleGetAllMashups(context);
        }
        
        var hasMashupIdPathParameter = request.PathParameters.TryGetValue("mashupId", out var mashupIdStr);
        uint? mashupId = hasMashupIdPathParameter ? uint.Parse(mashupIdStr!) : null;
        
        if (request is { HttpMethod: "GET", Resource: "/mashups/{mashupId}" } && hasMashupIdPathParameter)
        {
            context.Logger.LogInformation("Get mashup with ID: {mashupId}", mashupId);
            return await HandleGetMashupById(mashupId ?? 0, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/albums" })
        {
            context.Logger.LogInformation("Requested all albums");
            return await HandleGetAllAlbums(context);
        }
        
        var hasAlbumIdPathParameter = request.PathParameters.TryGetValue("albumId", out var albumIdStr);
        uint? albumId = hasAlbumIdPathParameter ? uint.Parse(albumIdStr!) : null;
        
        if (request is { HttpMethod: "GET", Resource: "/albums/{albumId}" } && hasAlbumIdPathParameter)
        {
            context.Logger.LogInformation("Get album with ID: {albumId}", albumId);
            return await HandleGetAlbumById(albumId ?? 0, context);
        }
        
        context.Logger.LogError("There is no implementation for a HTTP '{method}' request with path '{path}'", request.HttpMethod, request.Path);

        var noRouteFoundError = new ErrorResponse
        {
            Message = "Invalid request path!"
        };
        var response = new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.NotFound,
            Body = JsonSerializer.Serialize(noRouteFoundError, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };

        return response;
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSetlist(uint setlistId, ILambdaContext context)
    {
        try
        {
            var setlistDto = await _setlistService.GetCompleteSetlist(setlistId);
            return Ok(setlistDto, SetlistDtoJsonContext.Default.SetlistDto, HttpMethod.Get);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSetlistEntry(uint setlistId, string entryId, ILambdaContext context)
    {
        try
        {
            var setlistEntryDto = await _setlistService.GetSetlistEntryAsync(setlistId, entryId);
            return Ok(setlistEntryDto, SetlistDtoJsonContext.Default.RawSetlistEntryDto, HttpMethod.Get);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSetlistHeaders(ILambdaContext context)
    {
        try
        {
            var setlists = await _setlistService.GetSetlistHeaders(context.GetCancellationToken()).ToListAsync();
            return Ok(setlists, SetlistDtoJsonContext.Default.ListSetlistHeaderDto, HttpMethod.Get);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSetlistsForConcert(string setlistId, ILambdaContext context)
    {
        try
        {
            var setlistDtos = await _setlistService.GetSetlistsForConcert(setlistId);
            return Ok((List<SetlistDto>)setlistDtos, SetlistDtoJsonContext.Default.ListSetlistDto, HttpMethod.Get);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSong(uint songId, ILambdaContext context)
    {
        try
        {
            var song = await _songService.GetSongById(songId);
            return Ok(song, SetlistDtoJsonContext.Default.SongDto, HttpMethod.Get);
        }
        catch (SongNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetAllSongs(ILambdaContext context)
    {
        try
        {
            var songs = await _songService.GetAllSongsAsync(context.GetCancellationToken()).ToListAsync();
            context.Logger.LogDebug("Found {songCount} songs", songs.Count);
            return Ok(songs, SetlistDtoJsonContext.Default.ListSongDto, HttpMethod.Get);
        }
        catch (SongNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetAllAlbums(ILambdaContext context)
    {
        try
        {
            var albums = await _albumService.GetAllAlbumsAsync(context.GetCancellationToken()).ToListAsync();
            context.Logger.LogDebug("Found {albumCount} albums", albums.Count);
            return Ok(albums, SetlistDtoJsonContext.Default.ListAlbumDto, HttpMethod.Get);
        }
        catch (SongNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    /// <summary>
    /// Returns the album
    /// </summary>
    /// <param name="albumId">ID of the album</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleGetAlbumById(uint albumId, ILambdaContext context)
    {
        try
        {
            var album = await _albumService.GetAlbumById(albumId);
            return Ok(album, SetlistDtoJsonContext.Default.AlbumDto, HttpMethod.Get);
        }
        catch (AlbumNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetVariantsOfSong(uint songId, ILambdaContext context)
    {
        try
        {
            var variants = await _songService.GetVariantsOfSong(songId);
            return Ok(variants, SetlistDtoJsonContext.Default.ListSongVariantDto, HttpMethod.Get);
        }
        catch (SongNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }


    /// <summary>
    /// Return an API response with status 404
    /// </summary>
    /// <param name="message"></param>
    /// <param name="corsMethods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    [Obsolete]
    private static APIGatewayProxyResponse HandleNotFoundException(string message, string corsMethods, ILambdaLogger logger)
    {
        var internalErrorResponse = new ErrorResponse
        {
            Message = message
        };

        logger.LogError("Handle not found error: {message}", internalErrorResponse.Message);

        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.NotFound,
            Body = JsonSerializer.Serialize(internalErrorResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", corsMethods }
            }
        };
    }
    
    /// <summary>
    /// Return an API response with status 404
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="corsMethods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static APIGatewayProxyResponse HandleNotFoundException(SetlistServiceException exception, HttpMethod[] corsMethods, ILambdaLogger logger)
    {
        logger.LogError("Handle not found error: {message}", exception.Message);
        return NotFound(exception.Message, CacheControlHeaderConfig.Default, corsMethods);
    }
    
    /// <summary>
    /// Returns all mashups from the database
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleGetAllMashups(ILambdaContext context)
    {
        var cancellationToken = context.GetCancellationToken();
        var mashups = await _songService.GetAllSongMashupsAsync(cancellationToken).ToListAsync(cancellationToken);
        context.Logger.LogDebug("Found {count} mashups", mashups.Count);
        return Ok(mashups, SetlistDtoJsonContext.Default.ListSongMashupDto, HttpMethod.Get);
    }
    
    
    /// <summary>
    /// Returns the mashup
    /// </summary>
    /// <param name="mashupId">ID of the mashup</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleGetMashupById(uint mashupId, ILambdaContext context)
    {
        var mashup = await _songService.GetMashupByIdAsync(mashupId);
        if (mashup != null) 
            return Ok(mashup, SetlistDtoJsonContext.Default.SongMashupDto, HttpMethod.Get);
        
        context.Logger.LogError("Mashup with ID '{id}' not found!", mashupId);
        return NotFound($"The mashup with ID '{mashupId}' does not exist.", CacheControlHeaderConfig.Default, HttpMethod.Get);
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetActsInSetlist(uint setlistId, ILambdaContext context)
    {
        try
        {
            var setlistActs = await _setlistService.GetActsWithinSetlistAsync(setlistId).ToListAsync(context.GetCancellationToken());
            return Ok(setlistActs, SetlistDtoJsonContext.Default.ListSetlistActDto, HttpMethod.Get);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Get], context.Logger);
        }
    }
}