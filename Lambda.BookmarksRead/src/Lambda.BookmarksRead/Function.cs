using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Auth;
using Lambda.Common.ApiGateway;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.BookmarksRead;

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
        context.Logger.LogDebug($"Path: {request.Path}");
        
        if (request.Resource is "/concerts/{id}/bookmarks/count" or "/concerts/{id}/bookmarks/status"
            && request.PathParameters.TryGetValue("id", out var concertId))
        {
            context.Logger.LogInformation("Requested bookmark counts for a concert");
            return await ReturnBookmarkCountsForConcert(concertId!, request.GetUserId(), context.Logger);
        }

        var error = new ErrorResponse
        {
            Message = "Invalid route"
        };
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(error, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }


    private async Task<ConcertBookmark.BookmarkStatus> GetBookmarkStatusForUserAtConcert(string concertId,
        string? userId, ILambdaLogger logger)
    {
        if (userId == null)
        {
            return ConcertBookmark.BookmarkStatus.None;
        }
        
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks);
        config.IndexName = ConcertBookmark.UserBookmarksIndex;
        
        logger.LogDebug($"Query index: {config.IndexName}");
        
        var query = _dynamoDbContext.QueryAsync<ConcertBookmark>(
            userId, // PartitionKey value
            QueryOperator.Equal,
            [ concertId ],
            config);
        var bookmarkList = await query.GetRemainingAsync();
        return bookmarkList.FirstOrDefault()?.Status ?? ConcertBookmark.BookmarkStatus.None;
    }


    private IAsyncSearch<ConcertBookmark> QueryBookmarksFor(string concertId, ConcertBookmark.BookmarkStatus status, ILambdaLogger logger)
    {
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks);
        config.IndexName = "ConcertBookmarkStatusIndexV1";
        
        logger.LogDebug($"Query index: {config.IndexName}");
        
        return _dynamoDbContext.QueryAsync<ConcertBookmark>(
            concertId, // PartitionKey value
            QueryOperator.Equal,
            [status.ToString()],
            config);
    }


    private async Task<APIGatewayProxyResponse> ReturnBookmarkCountsForConcert(string concertId, string? currentUserId, ILambdaLogger logger)
    {
        var queryBookmarked = QueryBookmarksFor(concertId, ConcertBookmark.BookmarkStatus.Bookmarked, logger);
        var queryAttending = QueryBookmarksFor(concertId, ConcertBookmark.BookmarkStatus.Attending, logger);

        var bookmarkedTask = queryBookmarked.GetRemainingAsync();
        var attendingTask = queryAttending.GetRemainingAsync();
        var bookmarkStatusTask = GetBookmarkStatusForUserAtConcert(concertId, currentUserId, logger);
        
        await Task.WhenAll(bookmarkedTask, attendingTask, bookmarkStatusTask);
        var countBookmarked = bookmarkedTask.Result.Count;
        var countAttending = attendingTask.Result.Count;

        var response = new GetConcertBookmarkCountsResponse
        {
            Attending = countAttending,
            Bookmarked = countBookmarked,
            CurrentUserStatus = bookmarkStatusTask.Result
        };
        
        var json = JsonSerializer.Serialize(response, DataStructureJsonContext.Default.GetConcertBookmarkCountsResponse);
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