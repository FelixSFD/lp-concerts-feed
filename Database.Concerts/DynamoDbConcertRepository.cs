using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;

namespace Database.Concerts;

/// <summary>
/// Manages Concerts stored in DynamoDB
/// </summary>
public class DynamoDbConcertRepository : IConcertRepository
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider;

    public DynamoDbConcertRepository(DynamoDBContext dynamoDbContext, DynamoDbConfigProvider dbConfigProvider)
    {
        _dynamoDbContext = dynamoDbContext;
        _dbConfigProvider = dbConfigProvider;
        
        _dynamoDbContext.RegisterCustomConverters();
    }


    /// <summary>
    /// Returns an instance with default configuration
    /// </summary>
    /// <returns></returns>
    public static DynamoDbConcertRepository CreateDefault()
    {
        var ctx = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        return new DynamoDbConcertRepository(ctx, new DynamoDbConfigProvider());
    }

    
    /// <inheritdoc/>
    public async Task<Concert?> GetByIdAsync(string id)
    {
        return await _dynamoDbContext.LoadAsync<Concert>(id, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
    }
}