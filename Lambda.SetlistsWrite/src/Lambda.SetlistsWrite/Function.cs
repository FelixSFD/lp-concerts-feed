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
        _setlistService = new SetlistService(_setlistRepository, _concertRepository);
        
        // TODO: Enable authorization!
        /*var hasSetlistPermission = request.CanManageSetlists();
        if (!hasSetlistPermission)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }*/
        
        if (request.Body == null)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{\"message\": \"Request body not found\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
                }
            };
        }

        if (request is { HttpMethod: "POST", Path: "/setlists" })
        {
            context.Logger.LogInformation("Creating a setlist...");
            return await HandleCreateSetlist(request.Body, context);
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
}