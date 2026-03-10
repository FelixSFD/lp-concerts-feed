using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Database.Concerts;
using Database.Setlists;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;
using LPCalendar.DataStructure.Setlists;
using Microsoft.EntityFrameworkCore;
using Service.Setlists;
using Service.Setlists.Exceptions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.SetlistsWrite;

public class Function
{
    private IConcertRepository _concertRepository;
    private ISetlistRepository _setlistRepository;
    private ISetlistActRepository _setlistActRepository;
    private ISetlistEntryRepository _setlistEntryRepository;
    private ISongRepository _songRepository;
    private ISongVariantRepository _songVariantRepository;
    private ISongMashupRepository _songMashupRepository;
    private SetlistService _setlistService;
    private SongService _songService;

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
        _songRepository = new SqlSongRepository(dbContext);
        _songVariantRepository = new SqlSongVariantRepository(dbContext);
        _songMashupRepository = new SqlSongMashupRepository(dbContext);
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository, _songVariantRepository, _setlistActRepository, context.Logger);
        _songService = new SongService(_songRepository, _songVariantRepository, _songMashupRepository, context.Logger);
        
        context.Logger.LogInformation("Called {method} {path}", request.HttpMethod, request.Resource);

        request.PathParameters ??= new Dictionary<string, string>();
        
        var hasSetlistIdPathParameter = request.PathParameters.TryGetValue("setlistId", out var setlistIdStr);
        uint? setlistId = hasSetlistIdPathParameter ? uint.Parse(setlistIdStr!) : null;
        
        var hasSongIdPathParameter = request.PathParameters.TryGetValue("songId", out var songIdStr);
        uint? songId = hasSongIdPathParameter ? uint.Parse(songIdStr!) : null;

        if (request is { HttpMethod: "GET", Resource: "/setlists/{setlistId}" } && hasSetlistIdPathParameter)
        {
            context.Logger.LogInformation("Reading a setlist...");
            return await HandleGetSetlist(setlistId ?? 0, context);
        }
        
        if (request is { HttpMethod: "GET", Resource: "/songs/{songId}" } && hasSongIdPathParameter)
        {
            context.Logger.LogInformation("Requested song with ID: {songId}", songId);
            return await HandleGetSong(songId ?? 0, context);
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
            context.Logger.LogInformation("Get all mashups with ID: {mashupId}", mashupId);
            return await HandleGetMashupById(mashupId ?? 0, context);
        }
        
        // TODO: Enable authorization!
        /*var hasSetlistPermission = request.CanManageSetlists();
        if (!hasSetlistPermission)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }*/

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
        
        if (request is { HttpMethod: "DELETE", Resource: "/setlists/{setlistId}" } && hasSetlistIdPathParameter)
        {
            return await HandleDeleteSetlist(setlistId ?? 0, context);
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
        
        var hasSetlistEntryIdPathParameter = request.PathParameters.TryGetValue("setlistEntryId", out var setlistEntryId);
        
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
            var concertNotFound = new ErrorResponse
            {
                Message = $"The concert '{concertId}' does not exist!"
            };
            
            context.Logger.LogError(concertNotFound.Message);
            
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = JsonSerializer.Serialize(concertNotFound, DataStructureJsonContext.Default.ErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, POST" }
                }
            };
        }
        
        var responseDto = await _setlistService.CreateSetlistAsync(request);
        context.Logger.LogDebug("Successfully created setlist with ID: {id}", responseDto.Id);
        
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.Created,
            Body = JsonSerializer.Serialize(responseDto, SetlistDtoJsonContext.Default.CreateSetlistResponseDto),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, POST" }
            }
        };
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
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonSerializer.Serialize(responseDto, SetlistDtoJsonContext.Default.AddSongToSetlistResponseDto),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, POST" }
                }
            };
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
        
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonSerializer.Serialize(responseDto, SetlistDtoJsonContext.Default.AddSongVariantToSetlistResponseDto),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, POST" }
                }
            };
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
        
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, DELETE" }
            }
        };
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
        
        return new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, DELETE" }
            }
        };
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSetlist(uint setlistId, ILambdaContext context)
    {
        try
        {
            var setlistDto = await _setlistService.GetCompleteSetlist(setlistId);
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(setlistDto, SetlistDtoJsonContext.Default.SetlistDto),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        catch (SetlistNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, GET", context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetSong(uint songId, ILambdaContext context)
    {
        try
        {
            var song = await _songService.GetSongById(songId);

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(song, SetlistDtoJsonContext.Default.SongDto),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        catch (SongNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, GET", context.Logger);
        }
    }
    
    
    private async Task<APIGatewayProxyResponse> HandleGetVariantsOfSong(uint songId, ILambdaContext context)
    {
        try
        {
            var variants = await _songService.GetVariantsOfSong(songId);

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(variants, SetlistDtoJsonContext.Default.ListSongVariantDto),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        catch (SongNotFoundException e)
        {
            return HandleNotFoundException(e.Message, "OPTIONS, GET", context.Logger);
        }
    }


    /// <summary>
    /// Return an API response with status 404
    /// </summary>
    /// <param name="message"></param>
    /// <param name="corsMethods"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
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
    /// Create a new setlist in the database
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

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonSerializer.Serialize(responseDto, SetlistDtoJsonContext.Default.SongMashupDto),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, POST" }
                }
            };
        }
        catch (InvalidMashupException e)
        {
            return ReturnBadRequest(e.Message, "OPTIONS, POST");
        }
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

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(mashups, SetlistDtoJsonContext.Default.ListSongMashupDto),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
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
        if (mashup == null)
        {
            return HandleNotFoundException($"The mashup with ID '{mashupId}' does not exist.", "OPTIONS, GET", context.Logger);
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonSerializer.Serialize(mashup, SetlistDtoJsonContext.Default.SongMashupDto),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }
}