using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;

namespace Database.Concerts;

/// <summary>
/// Manages Concerts stored in DynamoDB
/// </summary>
public class DynamoDbConcertRepository : IConcertRepository
{
    private readonly ILambdaLogger _logger;
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider;

    public DynamoDbConcertRepository(DynamoDBContext dynamoDbContext, DynamoDbConfigProvider dbConfigProvider, ILambdaLogger logger)
    {
        _logger = logger;
        _dynamoDbContext = dynamoDbContext;
        _dbConfigProvider = dbConfigProvider;
        
        _dynamoDbContext.RegisterCustomConverters();
    }


    /// <summary>
    /// Returns an instance with default configuration
    /// </summary>
    /// <returns></returns>
    public static DynamoDbConcertRepository CreateDefault(ILambdaLogger logger)
    {
        var ctx = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        return new DynamoDbConcertRepository(ctx, new DynamoDbConfigProvider(), logger);
    }

    
    /// <inheritdoc/>
    public async Task<Concert?> GetByIdAsync(string id)
    {
        return await _dynamoDbContext.LoadAsync<Concert>(id, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
    }

    
    /// <inheritdoc/>
    public async Task<Concert?> GetNextAsync()
    {
        var now = DateTimeOffset.UtcNow.AddHours(-4);
        var dateNowStr = now.ToString("O");
        
        _logger.LogInformation("Query concerts after: {dateNowStr}", dateNowStr);
        
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            "PUBLISHED", // PartitionKey value
            QueryOperator.GreaterThanOrEqual,
            [new AttributeValue { S = dateNowStr }],
            config);

        var concerts = await query.GetRemainingAsync();
        return concerts.FirstOrDefault();
    }
}