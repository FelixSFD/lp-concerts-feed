using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Common.DynamoDb;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.DbConfig;

namespace Database.ConcertBookmarks;

/// <summary>
/// DynamoDb implementation for <see cref="IConcertBookmarkRepository"/>
/// </summary>
public class DynamoDbConcertBookmarkRepository(
    DynamoDBContext dynamoDbContext,
    DynamoDbConfigProvider dbConfigProvider,
    ILambdaLogger logger)
    : BaseDynamoDbRepository(dynamoDbContext, dbConfigProvider, logger), IConcertBookmarkRepository
{
    /// <summary>
    /// Returns an instance with default configuration
    /// </summary>
    /// <returns></returns>
    public static DynamoDbConcertBookmarkRepository CreateDefault(ILambdaLogger logger)
    {
        var ctx = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        return new DynamoDbConcertBookmarkRepository(ctx, new DynamoDbConfigProvider(), logger);
    }
    
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<ConcertBookmark> GetForUserAsync(string userId, ConcertBookmark.BookmarkStatus status)
    {
        Logger.LogDebug("Fetching ConcertBookmarks with status '{status}' for user: {userId}", status, userId);
        var config = DbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks);
        config.BackwardQuery = false;
        config.IndexName = "UserBookmarkStatusIndexV1";
        
        var query = DynamoDbContext.QueryAsync<ConcertBookmark>(
            userId, // PartitionKey value
            QueryOperator.Equal,
            [status.ToString()],
            config);
        var results = await query.GetRemainingAsync();
        foreach (var bookmark in results)
        {
            Logger.LogDebug("Bookmarked concert: {concert}", bookmark.ConcertId);
            yield return bookmark;
        }
        
        Logger.LogDebug("Finished fetching ConcertBookmarks for user: {userId}", userId);
    }
}