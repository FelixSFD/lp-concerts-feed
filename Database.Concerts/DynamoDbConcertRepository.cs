using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Common.Utils;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;

namespace Database.Concerts;

using DateRange = (DateTimeOffset? from, DateTimeOffset? to);

/// <summary>
/// Manages Concerts stored in DynamoDB
/// </summary>
public class DynamoDbConcertRepository : IConcertRepository
{
    private readonly ILambdaLogger _logger;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider;

    private DynamoDbConcertRepository(IAmazonDynamoDB dynamoDbClient, DynamoDBContext dynamoDbContext, DynamoDbConfigProvider dbConfigProvider, ILambdaLogger logger)
    {
        _logger = logger;
        _dynamoDbClient = dynamoDbClient;
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
        var client = new AmazonDynamoDBClient();
        var ctx = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        return new DynamoDbConcertRepository(client, ctx, new DynamoDbConfigProvider(), logger);
    }

    
    /// <inheritdoc/>
    public async Task<Concert?> GetByIdAsync(string id)
    {
        return await _dynamoDbContext.LoadAsync<Concert>(id, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
    }

    
    /// <inheritdoc/>
    public IAsyncEnumerable<Concert> GetByIds(IEnumerable<string> ids)
    {
        return ids
            .Chunk(100)
            .ToAsyncEnumerable()
            .SelectMany(GetBatchByIds);
    }


    private async IAsyncEnumerable<Concert> GetBatchByIds(IEnumerable<string> ids)
    {
        var batchGet = _dynamoDbContext.CreateBatchGet<Concert>(_dbConfigProvider.GetBatchGetConfigFor(DynamoDbConfigProvider.Table.Concerts));
        foreach (var id in ids)
        {
            batchGet.AddKey(id);
        }

        await batchGet.ExecuteAsync();
        foreach (var concert in batchGet.Results.OfType<Concert>())
        {
            yield return concert;
        }
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

    
    /// <inheritdoc/>
    public async IAsyncEnumerable<Concert> GetConcertsAsync(string? filterTour = null, DateRange? dateRange = null)
    {
        _logger.LogDebug("Query filtered concerts: DateRange ({from} to {to}); Tour: {tour}", dateRange?.from.ToString() ?? "null", dateRange?.to.ToString() ?? "null", filterTour);
        var searchStartDate = dateRange?.from ?? DateTimeOffset.Now;
        if (dateRange?.from == null && dateRange?.to != null)
        {
            _logger.LogDebug("date_from is not set, but date_to is. Will search for historic shows as well", dateRange);
            searchStartDate = DateTimeOffset.MinValue;
        }
        
        // if no end is specified, use max value to search for all shows
        var searchEndDate = dateRange?.to ?? DateTimeOffset.MaxValue;
        
        var searchStartDateStr = searchStartDate.ToString("O");
        var searchEndDateStr = searchEndDate.ToString("O");
        _logger.LogInformation("SCAN filtered concerts between {start} and {end}", searchStartDateStr, searchEndDateStr);

        var config = _dbConfigProvider.GetScanConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        // build Scan Conditions
        List<ScanCondition> conditions =
        [
            new("Status", ScanOperator.Equal, "PUBLISHED"),
            new("PostedStartTime", ScanOperator.Between, searchStartDate,  searchEndDate),
        ];

        if (filterTour != null)
        {
            _logger.LogDebug("Add filter for TourName = '{tourName}'",  filterTour);
            if (string.IsNullOrEmpty(filterTour))
            {
                _logger.LogDebug("Filter for shows without tour");
                conditions.Add(new ScanCondition("TourName", ScanOperator.IsNull));
            }
            else
            {
                conditions.Add(new ScanCondition("TourName", ScanOperator.Equal, filterTour));
            }
        }
        
        _logger.LogDebug("Start returning the results...");
        var query = _dynamoDbContext.ScanAsync<Concert>(conditions, config);
            
        var nextSetAsync = await query.GetRemainingAsync();
        
        foreach (var concert in nextSetAsync)
        {
            yield return concert;
        }

        _logger.LogDebug("Finished returning all results.");
    }

    
    /// <inheritdoc/>
    public async IAsyncEnumerable<Concert> GetConcertsAsync(DateTimeOffset afterDate)
    {
        var searchStartDateStr = afterDate.ToString("O");
            
        _logger.LogInformation("Query concerts after: {time}", searchStartDateStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.BackwardQuery = false;
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            "PUBLISHED", // PartitionKey value
            QueryOperator.GreaterThanOrEqual,
            [new AttributeValue { S = searchStartDateStr }],
            config);

        var concerts = await query.GetRemainingAsync() ?? [];
        foreach (var concert in concerts)
        {
            yield return concert;
        }
    }

    
    /// <inheritdoc/>
    public async IAsyncEnumerable<Concert> GetConcertsChangedAfterAsync(DateTimeOffset changedAfterDate)
    {
        var searchStartDateStr = changedAfterDate.ToString("O");
            
        _logger.LogInformation("Query concerts CHANGED after: {time}", searchStartDateStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.BackwardQuery = false;
        config.IndexName = Concert.LastChangeTimeGlobalIndex;
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            Concert.StatusPublished, // PartitionKey value
            QueryOperator.GreaterThan,
            [new AttributeValue { S = searchStartDateStr }],
            config);

        var concerts = await query.GetRemainingAsync() ?? [];
        foreach (var concert in concerts)
        {
            yield return concert;
        }
    }
    
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<Concert> GetConcertsDeletedAfterAsync(DateTimeOffset deletedAfterDate)
    {
        var searchStartDateStr = deletedAfterDate.ToString("O");
            
        _logger.LogInformation("Query concerts DELETED after: {time}", searchStartDateStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.BackwardQuery = false;
        config.IndexName = Concert.DeletedConcertsGlobalIndex;
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            Concert.StatusDeleted, // PartitionKey value
            QueryOperator.GreaterThan,
            [new AttributeValue { S = searchStartDateStr }],
            config);

        var concerts = await query.GetRemainingAsync() ?? [];
        foreach (var concert in concerts)
        {
            yield return concert;
        }
    }


    /// <inheritdoc/>
    public async Task<DateTimeOffset?> GetLastChangedAsync()
    {
        _logger.LogInformation("Current client: {client}", _dynamoDbClient);
        var queryRequest = new QueryRequest
        {
            TableName = DynamoDbConfigProvider.GetTableNameFor(DynamoDbConfigProvider.Table.Concerts),
            IndexName = Concert.LastChangeTimeGlobalIndex,
            KeyConditionExpression = "#col_status = :cs",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":cs"] = new()
                {
                    S = Concert.StatusPublished
                }
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#col_status"] = "Status"
            },
            ScanIndexForward = false,
            ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
            Limit = 1
        };
        var lastChangedResponse = await _dynamoDbClient.QueryAsync(queryRequest);
        _logger.LogDebug("Used capacity units to read LastChanged: {rcu}", lastChangedResponse?.ConsumedCapacity.CapacityUnits ?? double.MinValue);
        var lastChangedEntry = lastChangedResponse?.Items.FirstOrDefault();
        var lastChangedConcert = AttributeMapToConcert(lastChangedEntry);
        var lastChanged = lastChangedConcert?.LastChange ?? DateTimeOffset.MinValue;
        
        queryRequest.IndexName = Concert.DeletedConcertsGlobalIndex;
        var deletedAtResponse = await _dynamoDbClient.QueryAsync(queryRequest);
        _logger.LogDebug("Used capacity units to read DeletedAt: {rcu}", deletedAtResponse?.ConsumedCapacity.CapacityUnits);
        var lastDeletedEntry = deletedAtResponse?.Items.FirstOrDefault();
        var lastDeletedConcert = AttributeMapToConcert(lastDeletedEntry);
        var lastDeleted = lastDeletedConcert?.DeletedAt ?? DateTimeOffset.MinValue;

        return DateTimeOffsetExtensions.Max(lastChanged, lastDeleted);
    }


    private Concert? AttributeMapToConcert(Dictionary<string, AttributeValue>? attributes)
    {
        if (attributes == null)
            return null;
        
        var doc = Document.FromAttributeMap(attributes);
        return _dynamoDbContext.FromDocument<Concert>(doc);
    }


    /// <inheritdoc/>
    public async Task SaveAsync(Concert concert)
    {
        await FixNonOverridableFields(concert);
        await _dynamoDbContext.SaveAsync(concert, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.Concerts));
        _logger.LogDebug("Concert '{id}' has been saved.", concert.Id);
    }


    /// <summary>
    /// Deletes a concert by setting the status to DELETED
    /// </summary>
    /// <param name="concert">Concert to delete</param>
    public async Task DeleteAsync(Concert concert)
    {
        concert.Status = Concert.StatusDeleted;
        concert.DeletedAt = DateTimeOffset.UtcNow;
        await _dynamoDbContext.SaveAsync(concert, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.Concerts));
        _logger.LogInformation("Concert '{id}' has been DELETED.", concert.Id);
    }
    
    
    /// <summary>
    /// Overwrite fields that can't be set by the clients
    /// </summary>
    /// <param name="concert"></param>
    private async Task FixNonOverridableFields(Concert concert)
    {
        var existing = await GetByIdAsync(concert.Id);
        if (existing != null)
        {
            concert.ScheduleImageFile = existing.ScheduleImageFile;
        }
        
        concert.LastChange = DateTimeOffset.UtcNow;
    }
}