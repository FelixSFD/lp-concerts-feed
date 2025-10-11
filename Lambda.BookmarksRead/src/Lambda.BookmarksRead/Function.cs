using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.BookmarksRead;

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
        context.Logger.LogDebug($"Path: {request.Path}");
        context.Logger.LogDebug($"Request: {JsonSerializer.Serialize(request)}");
        
        if (request.Resource is "/concerts/{id}/bookmarks/count" or "/concerts/{id}/bookmarks/status"
            && request.PathParameters.TryGetValue("id", out var concertId))
        {
            context.Logger.LogInformation("Requested bookmark counts for a concert");
            return await ReturnBookmarkCountsForConcert(concertId!, request.GetUserId());
        }

        var error = new ErrorResponse
        {
            Message = "Invalid route"
        };
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(error),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }


    private async Task<ConcertBookmark.BookmarkStatus> GetBookmarkStatusForUserAtConcert(string concertId,
        string? userId)
    {
        if (userId == null)
        {
            return ConcertBookmark.BookmarkStatus.None;
        }
        
        var config = _dbOperationConfigProvider.GetConcertBookmarksConfigWithEnvTableName();
        config.IndexName = "UserBookmarksIndexV1";
        
        var query = _dynamoDbContext.QueryAsync<ConcertBookmark>(
            userId, // PartitionKey value
            QueryOperator.Equal,
            [ concertId ],
            config);
        var bookmarkList = await query.GetRemainingAsync();
        return bookmarkList.FirstOrDefault()?.Status ?? ConcertBookmark.BookmarkStatus.None;
    }


    private IAsyncSearch<ConcertBookmark> QueryBookmarksFor(string concertId, ConcertBookmark.BookmarkStatus status)
    {
        var config = _dbOperationConfigProvider.GetConcertBookmarksConfigWithEnvTableName();
        config.IndexName = "ConcertBookmarkStatusIndexV1";
        
        return _dynamoDbContext.QueryAsync<ConcertBookmark>(
            concertId, // PartitionKey value
            QueryOperator.Equal,
            [status.ToString()],
            config);
    }


    private async Task<APIGatewayProxyResponse> ReturnBookmarkCountsForConcert(string concertId, string? currentUserId)
    {
        var queryBookmarked = QueryBookmarksFor(concertId, ConcertBookmark.BookmarkStatus.Bookmarked);
        var queryAttending = QueryBookmarksFor(concertId, ConcertBookmark.BookmarkStatus.Attending);

        var bookmarkedTask = queryBookmarked.GetRemainingAsync();
        var attendingTask = queryAttending.GetRemainingAsync();
        var bookmarkStatusTask = GetBookmarkStatusForUserAtConcert(concertId, currentUserId);
        
        await Task.WhenAll(bookmarkedTask, attendingTask, bookmarkStatusTask);
        var countBookmarked = bookmarkedTask.Result.Count;
        var countAttending = attendingTask.Result.Count;

        var response = new GetConcertBookmarkCountsResponse
        {
            Attending = countAttending,
            Bookmarked = countBookmarked,
            CurrentUserStatus = bookmarkStatusTask.Result
        };
        
        var json = JsonSerializer.Serialize(response);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = json,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }
}