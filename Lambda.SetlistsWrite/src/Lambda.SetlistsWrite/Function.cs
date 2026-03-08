using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Database.Concerts;
using Database.Setlists;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Lambda.Auth;
using Lambda.SetlistsWrite.Services;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;
using LPCalendar.DataStructure.Setlists;
using Microsoft.EntityFrameworkCore;

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
    private SetlistService _setlistService;

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
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository,  _setlistActRepository, context.Logger);
        
        // TODO: Enable authorization!
        /*var hasSetlistPermission = request.CanManageSetlists();
        if (!hasSetlistPermission)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }*/
        
        if (request.Body == null)
        {
            return ReturnBadRequest("Missing request body!");
        }
        
        context.Logger.LogInformation("Called {method} {path}", request.HttpMethod, request.Resource);

        if (request is { HttpMethod: "POST", Resource: "/setlists" })
        {
            context.Logger.LogInformation("Creating a setlist...");
            return await HandleCreateSetlist(request.Body, context);
        }
        
        if (request is { HttpMethod: "POST", Resource: "/setlists/{setlistId}/songs" } && request.PathParameters.TryGetValue("setlistId", out var setlistId))
        {
            context.Logger.LogInformation("Adding a song to the setlist with ID '{setlistId}' ...", setlistId);
            if (!string.IsNullOrEmpty(setlistId))
                return await HandleAddSongToSetlist(request.Body, context);
            
            context.Logger.LogError("Invalid setlist ID!");
            return ReturnBadRequest("Invalid setlist ID!");
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
    
    private async Task<APIGatewayProxyResponse> HandleAddSongToSetlist(string requestJson,
        ILambdaContext context)
    {
        var dto = JsonSerializer.Deserialize(requestJson, SetlistDtoJsonContext.Default.AddSongToSetlistRequestDto);
        if (dto != null)
            return await HandleAddSongToSetlist(dto, context);
        
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
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<APIGatewayProxyResponse> HandleAddSongToSetlist(AddSongToSetlistRequestDto request,
        ILambdaContext context)
    {
        var entryDto = await _setlistService.AddSongToSetlistAsync(request);
        if (entryDto == null)
        {
            var internalErrorResponse = new ErrorResponse
            {
                Message = $"An unknown error occurred while adding song setlist with ID: {request.SetlistId}"
            };
            
            context.Logger.LogError(internalErrorResponse.Message);
            
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonSerializer.Serialize(internalErrorResponse, DataStructureJsonContext.Default.ErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, POST" }
                }
            };
        }
        
        var responseDto = new AddSongToSetlistResponseDto(entryDto);
        
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
}