using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Common.Utils.Cache;
using Common.Utils.Cors;
using Common.WikiMedia.Repositories;
using Database.Concerts;
using Database.Setlists;
using Database.Setlists.Repositories;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;
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
        
        _concertRepository = DynamoDbConcertRepository.CreateDefault(context.Logger);
        var connectionString = Environment.GetEnvironmentVariable("SETLISTS_DB_CONNECTION_STRING") ?? throw new Exception("Missing environment variable 'SETLISTS_DB_CONNECTION_STRING'!");
        var stopwatch = Stopwatch.StartNew();
        var optBuilder = new DbContextOptionsBuilder<SetlistsDbContext>();
        optBuilder.UseMySQL(connectionString);
        var dbContext = new SetlistsDbContext(optBuilder.Options);
        stopwatch.Stop();
        context.Logger.LogDebug("Init duration of EFCore context: {duration} ms", stopwatch.ElapsedMilliseconds);
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
        _wikiMediaRepository = new WikiMediaRepository(new HttpClient(), LinkinpediaImportService.LinkinpediaRestApiBaseUrl); // TODO: env variable?
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
                return ReturnBadRequest("Missing request body!");
            }
            
            context.Logger.LogInformation("Creating a setlist...");
            return await HandleCreateSetlist(request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/songs" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return ReturnBadRequest("Missing request body!");
            }
            
            context.Logger.LogInformation("Adding a song to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddSongToSetlist(request.Body, setlistId ?? 0, context);
            
            context.Logger.LogError("Invalid setlist ID!");
            return ReturnBadRequest("Invalid setlist ID!");
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/variants" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return ReturnBadRequest("Missing request body!");
            }
            
            context.Logger.LogInformation("Adding a song variant to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddSongVariantToSetlist(request.Body, setlistId ?? 0, context);
            
            context.Logger.LogError("Invalid setlist ID!");
            return ReturnBadRequest("Invalid setlist ID!");
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/mashups" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return ReturnBadRequest("Missing request body!");
            }
            
            context.Logger.LogInformation("Adding a song mashup to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddSongMashupToSetlist(request.Body, setlistId ?? 0, context);
            
            context.Logger.LogError("Invalid setlist ID!");
            return ReturnBadRequest("Invalid setlist ID!");
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/custom" } && hasSetlistIdPathParameter)
        {
            if (request.Body == null)
            {
                return ReturnBadRequest("Missing request body!");
            }
            
            context.Logger.LogInformation("Adding a song mashup to the setlist with ID '{setlistId}' ...", setlistId);
            if (setlistId != null)
                return await HandleAddCustomEntryToSetlist(request.Body, setlistId ?? 0, context);
            
            context.Logger.LogError("Invalid setlist ID!");
            return ReturnBadRequest("Invalid setlist ID!");
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
                return ReturnBadRequest("Missing request body!");
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
                return ReturnBadRequest("Missing request body!");
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
            
            context.Logger.LogError("Invalid setlist entry ID!");
            return ReturnBadRequest("Invalid setlist entry ID!");
        }
        
        if (request is { HttpMethod: "DELETE", Resource: "/setlists/{setlistId}/entries/{setlistEntryId}" } 
            && hasSetlistIdPathParameter
            && hasSetlistEntryIdPathParameter)
        {
            context.Logger.LogInformation("Remove entry ID '{setlistEntryId}' from setlist '{setlistId}' ...", setlistEntryId, setlistId);
            if (!string.IsNullOrEmpty(setlistEntryId))
                return await HandleDeleteEntryFromSetlist(setlistEntryId, context);
            
            context.Logger.LogError("Invalid setlist entry ID!");
            return ReturnBadRequest("Invalid setlist entry ID!");
        }
        
        if (request is { HttpMethod: "POST", Resource: "/albums" })
        {
            if (request.Body == null)
            {
                return ReturnBadRequest("Missing request body!");
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

    private static APIGatewayProxyResponse ReturnBadRequest(string message, string corsMethods = "OPTIONS, GET, POST")
    {
        var noRouteFoundError = new ErrorResponse
        {
            Message = message
        };
        
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(noRouteFoundError, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", corsMethods }
            }
        };
    }

    private async Task<APIGatewayProxyResponse> HandleCreateSetlist(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.CreateSetlistRequestDto);
        if (dto != null)
            return await HandleCreateSetlist(dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }

    /// <summary>
    /// Create a new setlist in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateSetlist(CreateSetlistRequestDto request,
        ILambdaContext context)
    {
        var concertId = request.ConcertId;
        context.Logger.LogDebug("Checking if concert '{concertId}' exists...", concertId);
        var foundConcert = await _concertRepository.GetByIdAsync(concertId);
        if (foundConcert == null)
        {
            context.Logger.LogError("The concert '{concertId}' does not exist!", concertId);
            return NotFound($"The concert '{concertId}' does not exist!", HttpMethod.Post);
        }
        
        var responseDto = await _setlistService.CreateSetlistAsync(request);
        context.Logger.LogDebug("Successfully created setlist with ID: {id}", responseDto.Id);
        return Created(responseDto, SetlistDtoJsonContext.Default.CreateSetlistResponseDto, HttpMethod.Post);
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistHeader(uint setlistId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.UpdateSetlistHeaderRequestDto);
        if (dto != null)
            return await HandleUpdateSetlistHeader(setlistId, dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }
    
    /// <summary>
    /// Updates header information of a setlist in the database
    /// </summary>
    /// <param name="setlistId">ID of the setlist to update</param>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistHeader(uint setlistId, UpdateSetlistHeaderRequestDto request,
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
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleReorderEntries(uint setlistId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.ReorderSetlistEntriesRequestDto);
        if (dto != null)
            return await HandleReorderEntries(setlistId, dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }
    
    /// <summary>
    /// Reorderes all entries of a setlist
    /// </summary>
    /// <param name="setlistId">ID of the setlist to reorder</param>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleReorderEntries(uint setlistId, ReorderSetlistEntriesRequestDto request,
        ILambdaContext context)
    {
        try
        {
            var reorderedEntries = await _setlistService.ReorderSetlistEntriesAsync(setlistId, request.EntryIds);
            context.Logger.LogDebug("Successfully reordered setlist with ID: {id}", setlistId);
            var response = new ReorderSetlistEntriesResponseDto
            {
                ReorderedEntries = reorderedEntries.Select(DtoMapper.ToDto).ToList()
            };
            
            return Ok(response, SetlistDtoJsonContext.Default.ReorderSetlistEntriesResponseDto,
                CacheControlHeaderConfig.None);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistEntry(string requestJson, uint setlistId, string entryId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.UpdateSetlistEntryRequestDto);
        if (dto != null)
            return await HandleUpdateSetlistEntry(dto, setlistId, entryId, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }
    
    /// <summary>
    /// Updates a setlist entry
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="entryId">ID of the entry</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateSetlistEntry(UpdateSetlistEntryRequestDto request, uint setlistId, string entryId,
        ILambdaContext context)
    {
        try
        {
            await _setlistService.UpdateSetlistEntryAsync(setlistId, entryId, request);
            return NoContent(HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddCustomEntryToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.AddCustomEntryToSetlistRequestDto);
        if (dto != null)
            return await HandleAddCustomEntryToSetlist(dto, setlistId, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }
    
    /// <summary>
    /// Adds a new custom entry to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddCustomEntryToSetlist(AddCustomEntryToSetlistRequestDto request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddCustomEntryToSetlistAsync(request, setlistId);
            var responseDto = new AddCustomEntryToSetlistResponseDto(entryDto);
            return Created(responseDto, SetlistDtoJsonContext.Default.AddCustomEntryToSetlistResponseDto, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddSongToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.AddSongToSetlistRequestDto);
        if (dto != null)
            return await HandleAddSongToSetlist(dto, setlistId, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }
    
    /// <summary>
    /// Adds a new song to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongToSetlist(AddSongToSetlistRequestDto request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddSongToSetlistAsync(request, setlistId);
            var responseDto = new AddSongToSetlistResponseDto(entryDto);
            return Created(responseDto, SetlistDtoJsonContext.Default.AddSongToSetlistResponseDto, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleAddSongVariantToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.AddSongVariantToSetlistRequestDto);
        if (dto != null)
            return await HandleAddSongVariantToSetlist(dto, setlistId, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }
    
    /// <summary>
    /// Adds a new song to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongVariantToSetlist(AddSongVariantToSetlistRequestDto request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddSongVariantToSetlistAsync(request, setlistId);
            var responseDto = new AddSongVariantToSetlistResponseDto(entryDto);
            return Created(responseDto, SetlistDtoJsonContext.Default.AddSongVariantToSetlistResponseDto, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleAddSongMashupToSetlist(string requestJson, uint setlistId,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.AddSongMashupToSetlistRequestDto);
        if (dto != null)
            return await HandleAddSongMashupToSetlist(dto, setlistId, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }
    
    /// <summary>
    /// Adds a new song mashup to an existing setlist
    /// </summary>
    /// <param name="request"></param>
    /// <param name="setlistId">ID of the setlist</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongMashupToSetlist(AddSongMashupToSetlistRequestDto request, uint setlistId,
        ILambdaContext context)
    {
        try
        {
            var entryDto = await _setlistService.AddSongMashupToSetlistAsync(request, setlistId);
            var responseDto = new AddSongMashupToSetlistResponseDto(entryDto);
            return Created(responseDto, SetlistDtoJsonContext.Default.AddSongMashupToSetlistResponseDto, HttpMethod.Post);
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
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
                { "Access-Control-Allow-Origin", CorsHeaderFactory.AllowOriginValue },
                { "Access-Control-Allow-Methods", corsMethods }
            }
        };
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleCreateAlbum(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.CreateAlbumRequestDto);
        if (dto != null)
            return await HandleCreateAlbum(dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", CorsHeaderFactory.AllowOriginValue },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }

    /// <summary>
    /// Create a new album in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateAlbum(CreateAlbumRequestDto request,
        ILambdaContext context)
    {
        var responseDto = await _albumService.CreateAlbumAsync(request);
        context.Logger.LogDebug("Successfully created album with ID: {id}", responseDto.Id);
        return Created(responseDto, SetlistDtoJsonContext.Default.AlbumDto, HttpMethod.Post);
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateAlbum(uint albumId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.UpdateAlbumRequestDto);
        if (dto != null)
            return await HandleUpdateAlbum(albumId, dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
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
    private async Task<APIGatewayProxyResponse> HandleUpdateAlbum(uint albumId, UpdateAlbumRequestDto request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _albumService.UpdateAlbumAsync(albumId, request);
            context.Logger.LogDebug("Successfully updated album with ID: {id}", responseDto.Id);
            return Ok(responseDto, SetlistDtoJsonContext.Default.AlbumDto, HttpMethod.Post);
        }
        catch (AlbumNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, POST", context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleCreateMashup(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.CreateSongMashupRequestDto);
        if (dto != null)
            return await HandleCreateMashup(dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }

    /// <summary>
    /// Create a new mashup in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateMashup(CreateSongMashupRequestDto request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.CreateMashupOfSongsAsync(request);
            context.Logger.LogDebug("Successfully created mashup with ID: {id}", responseDto.Id);
            return Created(responseDto, SetlistDtoJsonContext.Default.SongMashupDto, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, "OPTIONS, POST");
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
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.UpdateSongMashupRequestDto);
        if (dto != null)
            return await HandleUpdateMashup(mashupId, dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }

    /// <summary>
    /// Updates a mashup in the database
    /// </summary>
    /// <param name="mashupId">ID of the mashup to update</param>
    /// <param name="request">new data</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateMashup(uint mashupId, UpdateSongMashupRequestDto request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.UpdateMashupAsync(mashupId, request);
            context.Logger.LogDebug("Successfully updated mashup with ID: {id}", responseDto.Id);
            return Ok(responseDto, SetlistDtoJsonContext.Default.SongMashupDto, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, "OPTIONS, POST");
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
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.CreateSongRequestDto);
        if (dto != null)
            return await HandleCreateSong(dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }

    /// <summary>
    /// Create a new song in the database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleCreateSong(CreateSongRequestDto request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.CreateSongAsync(request);
            context.Logger.LogDebug("Successfully created song with ID: {id}", responseDto.Id);
            return Created(responseDto, SetlistDtoJsonContext.Default.SongDto, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, "OPTIONS, POST");
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleUpdateSong(uint songId, string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.UpdateSongRequestDto);
        if (dto != null)
            return await HandleUpdateSong(songId, dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }

    /// <summary>
    /// Updates a song in the database
    /// </summary>
    /// <param name="songId">ID of the song to update</param>
    /// <param name="request">new data</param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleUpdateSong(uint songId, UpdateSongRequestDto request,
        ILambdaContext context)
    {
        try
        {
            var responseDto = await _songService.UpdateSongAsync(songId, request);
            context.Logger.LogDebug("Successfully updated song with ID: {id}", responseDto.Id);
            return Ok(responseDto, SetlistDtoJsonContext.Default.SongDto, HttpMethod.Post);
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, "OPTIONS, POST");
        }
    }
    
    private async Task<APIGatewayProxyResponse> HandleImportSetlist(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.ImportSetlistRequestDto);
        if (dto != null)
            return await HandleImportSetlist(dto, context);
        
        var badRequestResponse = new ErrorResponse
        {
            Message = "Failed to deserialize the request body"
        };
            
        context.Logger.LogError(badRequestResponse.Message);
            
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
    }

    /// <summary>
    /// Import a setlist from Linkinpedia
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleImportSetlist(ImportSetlistRequestDto request,
        ILambdaContext context)
    {
        context.Logger.LogDebug("Importing from URL: {url}", request.LinkinpediaUrl);
        var responseDto = await _linkinpediaImportService.GetImportPlanForSetlistFromPageAsync(request.LinkinpediaUrl.Split("/").Last());
        context.Logger.LogDebug("Successfully generated import instructions.");
        return Ok(responseDto, SetlistDtoJsonContext.Default.ImportSetlistPreviewDto, HttpMethod.Post);
    }
}