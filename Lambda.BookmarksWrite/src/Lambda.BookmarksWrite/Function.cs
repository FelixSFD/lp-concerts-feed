using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using Lambda.Auth;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.BookmarksWrite;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();
    
    
    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.Body == null)
        {
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
        var currentUserId = request.GetUserId();
        request.PathParameters.TryGetValue("id", out var concertId);

        if (currentUserId == null)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, PUT");
        }

        if (concertId == null)
        {
            var errResponse = new ErrorResponse
            {
                Message = "ConcertId is missing"
            };
            var bodyJson = JsonSerializer.Serialize(errResponse);
            return new APIGatewayProxyResponse()
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
        
        // try to find bookmark for this user and concert
        var config = _dbOperationConfigProvider.GetConcertBookmarksConfigWithEnvTableName();
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
        await _dynamoDbContext.SaveAsync(bookmark, _dbOperationConfigProvider.GetConcertBookmarksConfigWithEnvTableName());
        
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
    
    
    private ConcertBookmarkUpdateRequest MakeRequestFromJsonBody(string json)
    {
        var bookmark = JsonSerializer.Deserialize<ConcertBookmarkUpdateRequest>(json) ?? throw new InvalidDataContractException("JSON could not be parsed to ConcertBookmarkUpdateRequest!");
        return bookmark;
    }
}