using System.Diagnostics;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Common.Utils.Cache;
using Common.WikiMedia.Repositories;
using Database.Setlists;
using Database.Setlists.Repositories;
using Database.Tours;
using Database.Tours.Repositories;
using Lambda.Auth;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Import;
using Microsoft.EntityFrameworkCore;
using Service.Setlists;
using Service.Setlists.Exceptions;
using Service.Setlists.Importer;
using static Lambda.Common.ApiGateway.ApiGatewayResponseHelper;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.SetlistsWrite;

public class Function
{
    private readonly IAmazonSQS _sqsClient = new AmazonSQSClient();
    private IConcertRepository _concertRepository;
    private ISetlistRepository _setlistRepository;
    private ISetlistActRepository _setlistActRepository;
    private ISetlistEntryRepository _setlistEntryRepository;
    private IAlbumRepository _albumRepository;
    private ISongRepository _songRepository;
    private ISongVariantRepository _songVariantRepository;
    private ISongMashupRepository _songMashupRepository;
    private IWikiMediaRepository _wikiMediaRepository;
    private IWikitextParser _wikitextParser = new WikitextParser();
    private SetlistService _setlistService;
    private AlbumService _albumService;
    private SongService _songService;
    private LinkinpediaImportService _linkinpediaImportService;
    private ISetlistPushEventSender _setlistPushEventSender;

    public Function()
    {
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Called {method} {path}", request.HttpMethod, request.Resource);
        
        var isAuthenticated = request.IsAuthenticated();
        if (!isAuthenticated)
            return UnauthorizedResponseHelper.GetResponse($"OPTIONS, {request.HttpMethod}");
        
        var hasSetlistPermission = request.CanManageSetlists();
        if (!hasSetlistPermission)
        {
            return ForbiddenResponseHelper.GetResponse($"OPTIONS, {request.HttpMethod}");
        }
        
        var connectionString = Environment.GetEnvironmentVariable("SETLISTS_DB_CONNECTION_STRING") ?? throw new Exception("Missing environment variable 'SETLISTS_DB_CONNECTION_STRING'!");
        var stopwatch = Stopwatch.StartNew();
        var optBuilder = new DbContextOptionsBuilder<SetlistsDbContext>();
        optBuilder.UseMySQL(connectionString);
        var dbContext = new SetlistsDbContext(optBuilder.Options);
        var toursOptBuilder = new DbContextOptionsBuilder<ToursDbContext>();
        toursOptBuilder.UseMySQL(connectionString);
        var toursDbContext = new ToursDbContext(toursOptBuilder.Options);
        stopwatch.Stop();
        context.Logger.LogDebug("Init duration of EFCore context: {duration} ms", stopwatch.ElapsedMilliseconds);
        _concertRepository = new SqlConcertRepository(toursDbContext);
        _setlistRepository = new SqlSetlistRepository(dbContext);
        _setlistActRepository = new SqlSetlistActRepository(dbContext);
        _setlistEntryRepository = new SqlSetlistEntryRepository(dbContext);
        _albumRepository = new SqlAlbumRepository(dbContext);
        _songRepository = new SqlSongRepository(dbContext);
        _songVariantRepository = new SqlSongVariantRepository(dbContext);
        _songMashupRepository = new SqlSongMashupRepository(dbContext);
        _setlistPushEventSender = new SetlistPushEventSender(_sqsClient, context.Logger);
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository, _songVariantRepository, _songMashupRepository, _setlistActRepository, _setlistPushEventSender, context.Logger);
        _albumService = new AlbumService(_albumRepository, context.Logger);
        _songService = new SongService(_songRepository, _songVariantRepository, _songMashupRepository, context.Logger);
        _wikiMediaRepository = new WikiMediaRepository(new HttpClient(), LinkinpediaImportService.LinkinpediaRestApiBaseUrl, LinkinpediaImportService.LinkinpediaActionApiBaseUrl, null); // TODO: env variable?
        _linkinpediaImportService = new LinkinpediaImportService(_wikiMediaRepository, _wikitextParser, _songRepository, _songMashupRepository, context.Logger);

        request.PathParameters ??= new Dictionary<string, string>();
        
        var hasSetlistIdPathParameter = request.PathParameters.TryGetValue("setlistId", out var setlistIdStr);
        uint? setlistId = hasSetlistIdPathParameter ? uint.Parse(setlistIdStr!) : null;
        
        var hasSongIdPathParameter = request.PathParameters.TryGetValue("songId", out var songIdStr);
        uint? songId = hasSongIdPathParameter ? uint.Parse(songIdStr!) : null;
        
        var hasMashupIdPathParameter = request.PathParameters.TryGetValue("mashupId", out var mashupIdStr);
        uint? mashupId = hasMashupIdPathParameter ? uint.Parse(mashupIdStr!) : null;
        
        var hasDeletePermission = request.CanDeleteSongs();

        if (request is { HttpMethod: "POST", Resource: "/setlists" })
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Creating a setlist...");
            return await HandleCreateSetlist(request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/songs" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Adding a song to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddSongToSetlist(request.Body, setlistId ?? 0, context);
            
            return ReturnBadRequest("Invalid setlist ID!", HttpMethod.Post, context.Logger);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/variants" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Adding a song variant to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddSongVariantToSetlist(request.Body, setlistId ?? 0, context);
            
            return ReturnBadRequest("Invalid setlist ID!", HttpMethod.Post, context.Logger);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/mashups" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Adding a song mashup to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddSongMashupToSetlist(request.Body, setlistId ?? 0, context);
            
            return ReturnBadRequest("Invalid setlist ID!", HttpMethod.Post, context.Logger);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/custom" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Adding a song mashup to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddCustomEntryToSetlist(request.Body, setlistId ?? 0, context);
            
            return ReturnBadRequest("Invalid setlist ID!", HttpMethod.Post, context.Logger);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}" } && hasSetlistIdPathParameter)
        {
            return await HandleUpdateSetlistHeader(setlistId ?? 0, request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/reorder" } && hasSetlistIdPathParameter)
        {
            return await HandleReorderEntries(setlistId ?? 0, request.Body, context);
        }
        
        if (request is { HttpMethod: "DELETE", Resource: "/setlists/{setlistId}" } && hasSetlistIdPathParameter)
        {
            if (!hasDeletePermission)
            {
                return ForbiddenResponseHelper.GetResponse("OPTIONS, DELETE");
            }
            
            return await HandleDeleteSetlist(setlistId ?? 0, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/songs" })
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Creating a song...");
            return await HandleCreateSong(request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/songs/{songId}" } && hasSongIdPathParameter)
        {
            return await HandleUpdateSong(songId ?? 0, request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/mashups" })
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Creating a mashup...");
            return await HandleCreateMashup(request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/mashups/{mashupId}" } && hasMashupIdPathParameter)
        {
            return await HandleUpdateMashup(mashupId ?? 0, request.Body, context);
        }
        
        if (request is { HttpMethod: "DELETE", Resource: "/mashups/{mashupId}" } && hasMashupIdPathParameter)
        {
            if (!hasDeletePermission)
            {
                return ForbiddenResponseHelper.GetResponse("OPTIONS, DELETE");
            }
            
            return await HandleDeleteSongMashup(mashupId ?? 0, context);
        }
        
        if (request is { HttpMethod: "DELETE", Resource: "/songs/{songId}" } && hasSongIdPathParameter)
        {
            if (!hasDeletePermission)
            {
                return ForbiddenResponseHelper.GetResponse("OPTIONS, DELETE");
            }
            
            return await HandleDeleteSong(songId ?? 0, context);
        }
        
        var hasSetlistEntryIdPathParameter = request.PathParameters.TryGetValue("setlistEntryId", out var setlistEntryId);
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/entries/{setlistEntryId}" } 
            && hasSetlistIdPathParameter
            && hasSetlistEntryIdPathParameter)
        {
            context.Logger.LogInformation("Update entry ID '{setlistEntryId}' in setlist '{setlistId}' ...", setlistEntryId, setlistId);
            if (!string.IsNullOrEmpty(setlistEntryId))
                return await HandleUpdateSetlistEntry(request.Body, setlistId ?? 0, setlistEntryId, context);
            
            return ReturnBadRequest("Invalid setlist entry ID!", HttpMethod.Post, context.Logger);
        }
        
        if (request is { HttpMethod: "DELETE", Resource: "/setlists/{setlistId}/entries/{setlistEntryId}" } 
            && hasSetlistIdPathParameter
            && hasSetlistEntryIdPathParameter)
        {
            context.Logger.LogInformation("Remove entry ID '{setlistEntryId}' from setlist '{setlistId}' ...", setlistEntryId, setlistId);
            if (!string.IsNullOrEmpty(setlistEntryId))
                return await HandleDeleteEntryFromSetlist(setlistEntryId, context);
            
            return ReturnBadRequest("Invalid setlist entry ID!", HttpMethod.Delete, context.Logger);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/entries/{setlistEntryId}/extras" } 
            && hasSetlistIdPathParameter
            && hasSetlistEntryIdPathParameter)
        {
            context.Logger.LogInformation("Add extra to entry with ID '{setlistEntryId}' in setlist '{setlistId}' ...", setlistEntryId, setlistId);
            if (!string.IsNullOrEmpty(setlistEntryId))
                return await HandleAddSetlistEntrySongExtra(request.Body, setlistId ?? 0, setlistEntryId, context);
            
            return ReturnBadRequest("Invalid setlist entry ID!", HttpMethod.Post, context.Logger);
        }
        
        var hasSongExtraIdPathParameter = request.PathParameters.TryGetValue("songExtraId", out var songExtraId);
        
        if (request is { HttpMethod: "DELETE", Resource: "/setlists/{setlistId}/entries/{setlistEntryId}/extras/{songExtraId}" } 
            && hasSetlistIdPathParameter
            && hasSetlistEntryIdPathParameter
            && hasSongExtraIdPathParameter)
        {
            context.Logger.LogInformation("Removing extra '{extraId}' from entry with ID '{setlistEntryId}' in setlist '{setlistId}' ...", setlistEntryId, setlistId);
            
            if (string.IsNullOrEmpty(setlistEntryId))
                return ReturnBadRequest("Invalid setlist entry ID!", HttpMethod.Post, context.Logger);
            
            if (string.IsNullOrEmpty(songExtraId))
                return ReturnBadRequest("Invalid song extra ID!", HttpMethod.Post, context.Logger);
            
            return await HandleDeleteSongExtra(setlistEntryId, songExtraId, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/albums" })
        {
            if (request.Body == null)
            {
                return HandleMissingBody(HttpMethod.Post, context.Logger);
            }
            
            context.Logger.LogInformation("Creating an album...");
            return await HandleCreateAlbum(request.Body, context);
        }
        
        var hasAlbumIdPathParameter = request.PathParameters.TryGetValue("albumId", out var albumIdStr);
        uint? albumId = hasAlbumIdPathParameter ? uint.Parse(albumIdStr!) : null;
        
        if (request is { HttpMethod: "POST", Resource: "/albums/{albumId}" } && hasAlbumIdPathParameter)
        {
            return await HandleUpdateAlbum(albumId ?? 0, request.Body, context);
        }
        
        if (request is { HttpMethod: "DELETE", Resource: "/albums/{albumId}" } && hasAlbumIdPathParameter)
        {
            if (!hasDeletePermission)
            {
                return ForbiddenResponseHelper.GetResponse("OPTIONS, DELETE");
            }
            
            return await HandleDeleteAlbum(albumId ?? 0, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/concerts/{id}/setlists/import" })
        {
            return await HandleImportSetlist(request.Body, context);
        }
        
        context.Logger.LogError("There is no implementation for a HTTP '{method}' request with path '{path}'", request.HttpMethod, request.Path);
        return NotFound("Invalid request path!", CacheControlHeaderConfig.Long, HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete);
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
    /// Return an API response with status 400 and a message that the request body is missing.
    /// </summary>
    /// <param name="corsMethod"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static APIGatewayProxyResponse HandleMissingBody(HttpMethod corsMethod, ILambdaLogger logger) 
        => ReturnBadRequest("Missing request body!", [corsMethod], logger);
    
    /// <summary>
    /// Return an API response with status 400
    /// </summary>
    /// <param name="message">error message</param>
    /// <param name="corsMethod"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static APIGatewayProxyResponse ReturnBadRequest(string message, HttpMethod corsMethod, ILambdaLogger logger) 
        => ReturnBadRequest(message, [corsMethod], logger);

    /// <summary>
    /// Return an API response with status 400
    /// </summary>
    /// <param name="message">error message</param>
    /// <param name="corsMethods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private static APIGatewayProxyResponse ReturnBadRequest(string message, HttpMethod[] corsMethods, ILambdaLogger logger)
    {
        logger.LogError("Handle bad request: {message}", message);
        return BadRequest(message, corsMethods);
    }

    private async Task<APIGatewayProxyResponse> HandleCreateSetlist(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.CreateSetlistRequestBo);
        if (dto != null)
            return await HandleCreateSetlist(dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Create a new setlist in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateSetlist(CreateSetlistRequestBo request,
        ILambdaContext context)
    {
        var concertId = request.ConcertId;
        context.Logger.LogDebug("Checking if concert '{concertId}' exists...", concertId);
        var foundConcert = await _concertRepository.GetByPrimaryKeyAsync(concertId);
        if (foundConcert == null)
        {
            context.Logger.LogError("The concert '{concertId}' does not exist!", concertId);
            return NotFound($"The concert '{concertId}' does not exist!", HttpMethod.Post);
        }
        
        var responseDto = await _setlistService.CreateSetlistAsync(request);
        context.Logger.LogDebug("Successfully created setlist with ID: {id}", responseDto.Id);
        return Created(responseDto, SetlistBoJsonContext.Default.CreateSetlistResponseBo, HttpMethod.Post);
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistHeader(uint setlistId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.UpdateSetlistHeaderRequestBo);
        if (dto != null)
            return await HandleUpdateSetlistHeader(setlistId, dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Updates header information of a setlist in the database
    /// </summary>
    /// <param name="setlistId">ID of the setlist to update</param>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistHeader(uint setlistId, UpdateSetlistHeaderRequestBo request,
        ILambdaContext context)
    {
        try
        {
            await _setlistService.UpdateSetlistHeader(setlistId, request);
            context.Logger.LogDebug("Successfully updated setlist with ID: {id}", setlistId);
            return NoContent(HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleReorderEntries(uint setlistId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.ReorderSetlistEntriesRequestBo);
        if (dto != null)
            return await HandleReorderEntries(setlistId, dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Reorderes all entries of a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist to reorder</param>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleReorderEntries(uint setlistId, ReorderSetlistEntriesRequestBo request,
        ILambdaContext context)
    {
        try
        {
            var reorderedEntries = await _setlistService.ReorderSetlistEntriesAsync(setlistId, request.EntryIds);
            context.Logger.LogDebug("Successfully reordered setlist with ID: {id}", setlistId);
            var response = new ReorderSetlistEntriesResponseBo
            {
                ReorderedEntries = reorderedEntries
            };
            
            return Ok(response, SetlistBoJsonContext.Default.ReorderSetlistEntriesResponseBo,
                CacheControlHeaderConfig.None);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistEntry(string requestJson, uint setlistId, string entryId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.UpdateSetlistEntryRequestBo);
        if (dto != null)
            return await HandleUpdateSetlistEntry(dto, setlistId, entryId, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Updates a setlist entry
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="entryId">ID of the entry</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistEntry(UpdateSetlistEntryRequestBo request, uint setlistId, string entryId,
        ILambdaContext context)
    {
        try
        {
            await _setlistService.UpdateSetlistEntryAsync(setlistId, entryId, request);
            return NoContent(HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddCustomEntryToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.AddCustomEntryToSetlistRequestBo);
        if (dto != null)
            return await HandleAddCustomEntryToSetlist(dto, setlistId, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Adds a new custom entry to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddCustomEntryToSetlist(AddCustomEntryToSetlistRequestBo request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddCustomEntryToSetlistAsync(request, setlistId);
            var responseDto = new AddCustomEntryToSetlistResponseBo(entryDto);
            return Created(responseDto, SetlistBoJsonContext.Default.AddCustomEntryToSetlistResponseBo, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddSongToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.AddSongToSetlistRequestBo);
        if (dto != null)
            return await HandleAddSongToSetlist(dto, setlistId, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Adds a new song to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongToSetlist(AddSongToSetlistRequestBo request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddSongToSetlistAsync(request, setlistId);
            var responseDto = new AddSongToSetlistResponseBo(entryDto);
            return Created(responseDto, SetlistBoJsonContext.Default.AddSongToSetlistResponseBo, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleAddSongVariantToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.AddSongVariantToSetlistRequestBo);
        if (dto != null)
            return await HandleAddSongVariantToSetlist(dto, setlistId, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Adds a new song to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongVariantToSetlist(AddSongVariantToSetlistRequestBo request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddSongVariantToSetlistAsync(request, setlistId);
            var responseDto = new AddSongVariantToSetlistResponseBo(entryDto);
            return Created(responseDto, SetlistBoJsonContext.Default.AddSongVariantToSetlistResponseBo, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddSongMashupToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.AddSongMashupToSetlistRequestBo);
        if (dto != null)
            return await HandleAddSongMashupToSetlist(dto, setlistId, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Adds a new song mashup to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongMashupToSetlist(AddSongMashupToSetlistRequestBo request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddSongMashupToSetlistAsync(request, setlistId);
            var responseDto = new AddSongMashupToSetlistResponseBo(entryDto);
            return Created(responseDto, SetlistBoJsonContext.Default.AddSongMashupToSetlistResponseBo, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    /// <summary>
    /// Removes a setlist
    /// </summary>
    /// <param name="setlistId">unique ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns>HTTP response</returns>
    private async Task<APIGatewayProxyResponse> HandleDeleteSetlist(uint setlistId, ILambdaContext context)
    {
        context.Logger.LogInformation("Deleting setlist with ID: {setlistEntryId}", setlistId);
        await _setlistService.RemoveSetlist(setlistId);
        return NoContent(HttpMethod.Delete);
    }
    
    /// <summary>
    /// Removes an entry from the setlist
    /// </summary>
    /// <param name="setlistEntryId">unique ID of the entry</param>
    /// <param name="context"></param>
    /// <returns>HTTP response</returns>
    private async Task<APIGatewayProxyResponse> HandleDeleteEntryFromSetlist(string setlistEntryId, ILambdaContext context)
    {
        context.Logger.LogInformation("Deleting setlist entry with ID: {setlistEntryId}", setlistEntryId);
        await _setlistService.RemoveSetlistEntry(setlistEntryId);
        return NoContent(HttpMethod.Delete);
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleCreateAlbum(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.CreateAlbumRequestBo);
        if (dto != null)
            return await HandleCreateAlbum(dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Create a new album in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateAlbum(CreateAlbumRequestBo request,
        ILambdaContext context)
    {
        var responseDto = await _albumService.CreateAlbumAsync(request);
        context.Logger.LogDebug("Successfully created album with ID: {id}", responseDto.Id);
        return Created(responseDto, SetlistBoJsonContext.Default.AlbumBo, HttpMethod.Post);
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateAlbum(uint albumId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.UpdateAlbumRequestBo);
        if (dto != null)
            return await HandleUpdateAlbum(albumId, dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }
    
    /// <summary>
    /// Removes an album
    /// </summary>
    /// <param name="albumId">unique ID of the album</param>
    /// <param name="context"></param>
    /// <returns>HTTP response</returns>
    private async Task<APIGatewayProxyResponse> HandleDeleteAlbum(uint albumId, ILambdaContext context)
    {
        context.Logger.LogInformation("Deleting song with ID: {albumId}", albumId);
        await _albumService.DeleteAlbumWithIdAsync(albumId);
        return NoContent(HttpMethod.Delete);
    }
    
    /// <summary>
    /// Updates an album in the database
    /// </summary>
    /// <param name="albumId">ID of the album to update</param>
    /// <param name="request">new data</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateAlbum(uint albumId, UpdateAlbumRequestBo request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _albumService.UpdateAlbumAsync(albumId, request);
            context.Logger.LogDebug("Successfully updated album with ID: {id}", responseDto.Id);
            return Ok(responseDto, SetlistBoJsonContext.Default.AlbumBo, HttpMethod.Post);
        }
        catch (AlbumNotFoundException e)
        {
            return HandleNotFoundException(e, [HttpMethod.Post], context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleCreateMashup(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.CreateSongMashupRequestBo);
        if (dto != null)
            return await HandleCreateMashup(dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Create a new mashup in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateMashup(CreateSongMashupRequestBo request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.CreateMashupOfSongsAsync(request);
            context.Logger.LogDebug("Successfully created mashup with ID: {id}", responseDto.Id);
            return Created(responseDto, SetlistBoJsonContext.Default.SongMashupBo, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, HttpMethod.Post, context.Logger);
        }
    }
    
    /// <summary>
    /// Removes a song mashup
    /// </summary>
    /// <param name="mashupId">unique ID of the mashup</param>
    /// <param name="context"></param>
    /// <returns>HTTP response</returns>
    private async Task<APIGatewayProxyResponse> HandleDeleteSongMashup(uint mashupId, ILambdaContext context)
    {
        context.Logger.LogInformation("Deleting setlist entry with ID: {mashupId}", mashupId);
        await _songService.DeleteMashupWithIdAsync(mashupId);
        return NoContent(HttpMethod.Delete);
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateMashup(uint mashupId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.UpdateSongMashupRequestBo);
        if (dto != null)
            return await HandleUpdateMashup(mashupId, dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Updates a mashup in the database
    /// </summary>
    /// <param name="mashupId">ID of the mashup to update</param>
    /// <param name="request">new data</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateMashup(uint mashupId, UpdateSongMashupRequestBo request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.UpdateMashupAsync(mashupId, request);
            context.Logger.LogDebug("Successfully updated mashup with ID: {id}", responseDto.Id);
            return Ok(responseDto, SetlistBoJsonContext.Default.SongMashupBo, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, HttpMethod.Post, context.Logger);
        }
    }
    
    /// <summary>
    /// Removes a song
    /// </summary>
    /// <param name="songId">unique ID of the song</param>
    /// <param name="context"></param>
    /// <returns>HTTP response</returns>
    private async Task<APIGatewayProxyResponse> HandleDeleteSong(uint songId, ILambdaContext context)
    {
        context.Logger.LogInformation("Deleting song with ID: {songId}", songId);
        await _songService.DeleteSongWithIdAsync(songId);
        return NoContent(HttpMethod.Delete);
    }
    
    private async Task<APIGatewayProxyResponse> HandleCreateSong(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.CreateSongRequestBo);
        if (dto != null)
            return await HandleCreateSong(dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Create a new song in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateSong(CreateSongRequestBo request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.CreateSongAsync(request);
            context.Logger.LogDebug("Successfully created song with ID: {id}", responseDto.Id);
            return Created(responseDto, SetlistBoJsonContext.Default.SongBo, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, HttpMethod.Post, context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateSong(uint songId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.UpdateSongRequestBo);
        if (dto != null)
            return await HandleUpdateSong(songId, dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Updates a song in the database
    /// </summary>
    /// <param name="songId">ID of the song to update</param>
    /// <param name="request">new data</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateSong(uint songId, UpdateSongRequestBo request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.UpdateSongAsync(songId, request);
            context.Logger.LogDebug("Successfully updated song with ID: {id}", responseDto.Id);
            return Ok(responseDto, SetlistBoJsonContext.Default.SongBo, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, HttpMethod.Post, context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleImportSetlist(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.ImportSetlistRequestBo);
        if (dto != null)
            return await HandleImportSetlist(dto, context);
        
        return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
    }

    /// <summary>
    /// Import a setlist from Linkinpedia
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleImportSetlist(ImportSetlistRequestBo request,
        ILambdaContext context)
    {
        context.Logger.LogDebug("Importing from URL: {url}", request.LinkinpediaUrl);
        var responseDto = await _linkinpediaImportService.GetImportPlanForSetlistFromPageAsync(request.LinkinpediaUrl.Split("/").Last());
        context.Logger.LogDebug("Successfully generated import instructions.");
        return Ok(responseDto, SetlistBoJsonContext.Default.ImportSetlistPreviewBo, HttpMethod.Post);
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddSetlistEntrySongExtra(string requestJson, uint setlistId, string setlistEntryId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistBoJsonContext.Default.AddSongExtraToSetlistEntryRequestBo);
        if (dto == null)
            return ReturnBadRequest("Failed to deserialize the request body", HttpMethod.Post, context.Logger);
        
        return await HandleAddSetlistEntrySongExtra(dto, setlistId, setlistEntryId, context);
    }

    /// <summary>
    /// add an extra to a setlist entry
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSetlistEntrySongExtra(AddSongExtraToSetlistEntryRequestBo request, uint setlistId, string setlistEntryId,
        ILambdaContext context)
    {
        context.Logger.LogDebug("Adding extra to setlist entry: {entryId}", setlistEntryId);
        await _setlistService.AddSongExtraToSetlistEntry(request, setlistEntryId);
        context.Logger.LogDebug("Successfully added extra.");
        
        var entry = await _setlistService.GetSetlistEntryAsync(setlistId, setlistEntryId);
        var responseDto = new AddSongExtraToSetlistEntryResponseBo
        {
            Entry = entry
        };
        
        return Ok(responseDto, SetlistBoJsonContext.Default.AddSongExtraToSetlistEntryResponseBo, HttpMethod.Post);
    }
    
    /// <summary>
    /// Removes an extra from a setlist entry
    /// </summary>
    /// <param name="setlistEntryId">unique ID of the entry</param>
    /// <param name="songExtraId">unique ID of the extra</param>
    /// <param name="context"></param>
    /// <returns>HTTP response</returns>
    private async Task<APIGatewayProxyResponse> HandleDeleteSongExtra(string setlistEntryId, string songExtraId, ILambdaContext context)
    {
        context.Logger.LogInformation("Deleting extra with ID: {songExtraId}", songExtraId);
        await _setlistService.RemoveSongExtraFromSetlistEntry(setlistEntryId, songExtraId);
        return NoContent(HttpMethod.Delete);
    }
}