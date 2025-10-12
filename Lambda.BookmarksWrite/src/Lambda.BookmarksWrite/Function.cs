using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using Lambda.Auth;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.BookmarksWrite;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    
    
    public Function()
    {
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.Body == null)
        {
            context.Logger.LogInformation("Request body is null");
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{\"message\": \"Request body not found\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
                }
            };
        }

        var parsedRequestBody = MakeRequestFromJsonBody(request.Body);
        context.Logger.LogDebug($"User sets status to: {parsedRequestBody.Status}");
        var currentUserId = request.GetUserId();
        request.PathParameters.TryGetValue("id", out var concertId);

        if (currentUserId == null)
        {
            context.Logger.LogInformation("No userId found");
            return ForbiddenResponseHelper.GetResponse("OPTIONS, PUT");
        }

        if (concertId == null)
        {
            context.Logger.LogInformation("No concertId found");
            
            var errResponse = new ErrorResponse
            {
                Message = "ConcertId is missing"
            };
            var bodyJson = JsonSerializer.Serialize(errResponse, DataStructureJsonContext.Default.ErrorResponse);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = bodyJson,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
                }
            };
        }
        
        // verify that concert actually exists
        var concert = await GetConcertById(concertId);
        if (concert == null)
        {
            context.Logger.LogInformation("Concert '{id}' not found", concertId);
            
            var errResponse = new ErrorResponse
            {
                Message = "Concert does not exist"
            };
            var bodyJson = JsonSerializer.Serialize(errResponse, DataStructureJsonContext.Default.ErrorResponse);
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = bodyJson,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
                }
            };
        }
        
        // try to find bookmark for this user and concert
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks);
        config.IndexName = "UserBookmarksIndexV1";
        
        var query = _dynamoDbContext.QueryAsync<ConcertBookmark>(
            currentUserId, // PartitionKey value
            QueryOperator.Equal,
            [ concertId ],
            config);
        
        var bookmarkList = await query.GetRemainingAsync();
        var bookmark = bookmarkList?.FirstOrDefault();
        
        // create or update model
        if (bookmark == null)
        {
            context.Logger.LogDebug("No bookmark found for user '{user}' on concert '{1}'. Create new one with status: {2}", currentUserId, concertId, parsedRequestBody.Status);
            bookmark = new ConcertBookmark
            {
                ConcertId = concertId,
                UserId = currentUserId,
                Status = parsedRequestBody.Status,
            };
        }
        else
        {
            bookmark.Status = parsedRequestBody.Status;
        }
        
        // write to DB
        await _dynamoDbContext.SaveAsync(bookmark, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks));
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
            }
        };
    }
    
    
    private static ConcertBookmarkUpdateRequest MakeRequestFromJsonBody(string json)
    {
        var bookmark = JsonSerializer.Deserialize(json, DataStructureJsonContext.Default.ConcertBookmarkUpdateRequest) ?? throw new InvalidDataContractException("JSON could not be parsed to ConcertBookmarkUpdateRequest!");
        return bookmark;
    }


    /// <summary>
    /// Gets a concert by its ID
    /// </summary>
    /// <param name="concertId"></param>
    /// <returns></returns>
    private async Task<Concert?> GetConcertById(string concertId)
    {
        return await _dynamoDbContext.LoadAsync<Concert>(concertId, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
    }
}