using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Common.Utils;
using Database.Concerts.Models;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;

namespace Database.Concerts;

using DateRange = (DateTimeOffset? from, DateTimeOffset? to);
using DbDocument = Dictionary<string, AttributeValue>;

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
    public async Task<ConcertModel?> GetByIdAsync(string id)
    {
        return await _dynamoDbContext.LoadAsync<ConcertModel>(id, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
    }

    
    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertModel> GetByIds(IEnumerable<string> ids)
    {
        return ids
            .Chunk(100)
            .ToAsyncEnumerable()
            .SelectMany(GetBatchByIds);
    }


    private async IAsyncEnumerable<ConcertModel> GetBatchByIds(IEnumerable<string> ids)
    {
        var batchGet = _dynamoDbContext.CreateBatchGet<ConcertModel>(_dbConfigProvider.GetBatchGetConfigFor(DynamoDbConfigProvider.Table.Concerts));
        foreach (var id in ids)
        {
            batchGet.AddKey(id);
        }

        await batchGet.ExecuteAsync();
        foreach (var concert in batchGet.Results.OfType<ConcertModel>())
        {
            yield return concert;
        }
    }


    /// <inheritdoc/>
    public async Task<ConcertModel?> GetNextAsync()
    {
        var now = DateTimeOffset.UtcNow.AddHours(-4);
        var dateNowStr = now.ToString("O");
        
        _logger.LogInformation("Query concerts after: {dateNowStr}", dateNowStr);
        
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<ConcertModel>(
            "PUBLISHED", // PartitionKey value
            QueryOperator.GreaterThanOrEqual,
            [new AttributeValue { S = dateNowStr }],
            config);

        var concerts = await query.GetRemainingAsync();
        return concerts.FirstOrDefault(c => c.ConcertStatus != nameof(ConcertDto.ConcertStatusValue.Cancelled));
    }

    
    /// <inheritdoc/>
    public async IAsyncEnumerable<ConcertModel> GetConcertsAsync(string? filterTour = null, DateRange? dateRange = null)
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
        var query = _dynamoDbContext.ScanAsync<ConcertModel>(conditions, config);
            
        var nextSetAsync = await query.GetRemainingAsync();
        
        foreach (var concert in nextSetAsync)
        {
            yield return concert;
        }

        _logger.LogDebug("Finished returning all results.");
    }


    /// <inheritdoc/>
    public async IAsyncEnumerable<ConcertModel> GetConcertsByStatusAsync(string concertStatus, DateRange? dateRange = null)
    {
        _logger.LogDebug("Query concerts with status '{status}': DateRange ({from} to {to}); Tour: {tour}", concertStatus, dateRange?.from.ToString() ?? "null", dateRange?.to.ToString() ?? "null");
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
        _logger.LogInformation("QUERY filtered concerts between {start} and {end}", searchStartDateStr, searchEndDateStr);
        
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.IndexName = ConcertModel.ConcertStatusGlobalIndex;
        
        var query = _dynamoDbContext.QueryAsync<ConcertModel>(
            concertStatus,
            QueryOperator.Between,
            [new AttributeValue { S = searchStartDateStr }, new AttributeValue { S = searchEndDateStr }],
            config);
        
        _logger.LogDebug("Start returning the results...");
        var concerts = await query.GetRemainingAsync() ?? [];
        foreach (var concert in concerts)
        {
            yield return concert;
        }
        _logger.LogDebug("Finished returning all results.");
    }

    
    /// <inheritdoc/>
    public async IAsyncEnumerable<ConcertModel> GetConcertsAsync(DateTimeOffset afterDate)
    {
        var searchStartDateStr = afterDate.ToString("O");
            
        _logger.LogInformation("Query concerts after: {time}", searchStartDateStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.BackwardQuery = false;
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<ConcertModel>(
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
    public async IAsyncEnumerable<ConcertModel> GetConcertsChangedAfterAsync(DateTimeOffset changedAfterDate)
    {
        var searchStartDateStr = changedAfterDate.ToString("O");
            
        _logger.LogInformation("Query concerts CHANGED after: {time}", searchStartDateStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.BackwardQuery = false;
        config.IndexName = ConcertModel.LastChangeTimeGlobalIndex;
        
        var query = _dynamoDbContext.QueryAsync<ConcertModel>(
            ConcertModel.StatusPublished, // PartitionKey value
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
    public async IAsyncEnumerable<ConcertModel> GetConcertsDeletedAfterAsync(DateTimeOffset deletedAfterDate)
    {
        var searchStartDateStr = deletedAfterDate.ToString("O");
            
        _logger.LogInformation("Query concerts DELETED after: {time}", searchStartDateStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.BackwardQuery = false;
        config.IndexName = ConcertModel.DeletedConcertsGlobalIndex;
        
        var query = _dynamoDbContext.QueryAsync<ConcertModel>(
            ConcertModel.StatusDeleted, // PartitionKey value
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
        var lastChangedConcertTask =
            GetLastChangedOrDeletedConcertAsync(ConcertModel.StatusPublished, ConcertModel.LastChangeTimeGlobalIndex);
        var lastDeletedConcertTask = GetLastChangedOrDeletedConcertAsync(ConcertModel.StatusDeleted, ConcertModel.DeletedConcertsGlobalIndex);
        
        await Task.WhenAll(lastChangedConcertTask, lastDeletedConcertTask);
        _logger.LogDebug("Finished both queries.");
        
        var lastChanged = GetDateFromAttributes(nameof(ConcertModel.LastChange), lastChangedConcertTask.Result) ?? DateTimeOffset.MinValue;
        var lastDeleted = GetDateFromAttributes(nameof(ConcertModel.DeletedAt), lastDeletedConcertTask.Result) ?? DateTimeOffset.MinValue;

        return DateTimeOffsetExtensions.Max(lastChanged, lastDeleted);
    }


    private async Task<DbDocument?> GetLastChangedOrDeletedConcertAsync(string status, string indexName)
    {
        var queryRequest = new QueryRequest
        {
            TableName = DynamoDbConfigProvider.GetTableNameFor(DynamoDbConfigProvider.Table.Concerts),
            IndexName = indexName,
            KeyConditionExpression = "#col_status = :cs",
            ExpressionAttributeValues = new DbDocument
            {
                [":cs"] = new AttributeValue
                {
                    S = status
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
        
        var queryResponse = await _dynamoDbClient.QueryAsync(queryRequest);
        _logger.LogDebug("Used capacity units to read latest change/delete with status '{status}': {rcu}", status, queryResponse?.ConsumedCapacity.CapacityUnits ?? double.MinValue);
        var lastChangedEntry = queryResponse?.Items.FirstOrDefault();
        return lastChangedEntry;
    }


    private DateTimeOffset? GetDateFromAttributes(string key, DbDocument? attributes)
    {
        if (attributes == null)
            return null;
        
        var foundAttribute = attributes.TryGetValue(key, out var dateAttribute);
        if (!foundAttribute || dateAttribute == null)
        {
            _logger.LogWarning("The attribute '{key}' does not exist in the dictionary.", key);
            return null;
        }

        var parsedDate = DateTimeOffset.TryParse(dateAttribute.S, out var date);
        if (parsedDate)
            return date;
        
        _logger.LogError("Failed to parse the attribute '{key}' with S '{date}' to DateTimeOffset!", key, dateAttribute.S);
        return null;

    }


    private ConcertModel? AttributeMapToConcert(DbDocument? attributes)
    {
        _logger.LogTrace("Start mapping concert attributes...");
        if (attributes == null)
            return null;
        
        var doc = Document.FromAttributeMap(attributes);
        var concert = _dynamoDbContext.FromDocument<ConcertModel>(doc);
        _logger.LogTrace("Finished mapping concert: {id}", concert.Id);
        return concert;
    }


    /// <inheritdoc/>
    public async Task SaveAsync(ConcertModel concert)
    {
        await FixNonOverridableFields(concert);
        await _dynamoDbContext.SaveAsync(concert, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.Concerts));
        _logger.LogDebug("Concert '{id}' has been saved.", concert.Id);
    }


    /// <summary>
    /// Deletes a concert by setting the status to DELETED
    /// </summary>
    /// <param name="concert">Concert to delete</param>
    public async Task DeleteAsync(ConcertModel concert)
    {
        concert.Status = ConcertModel.StatusDeleted;
        concert.DeletedAt = DateTimeOffset.UtcNow;
        await _dynamoDbContext.SaveAsync(concert, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.Concerts));
        _logger.LogInformation("Concert '{id}' has been DELETED.", concert.Id);
    }
    
    
    /// <summary>
    /// Overwrite fields that can't be set by the clients
    /// </summary>
    /// <param name="concert"></param>
    private async Task FixNonOverridableFields(ConcertModel concert)
    {
        var existing = await GetByIdAsync(concert.Id);
        if (existing != null)
        {
            concert.ScheduleImageFile = existing.ScheduleImageFile;

            _logger.LogDebug("Previous CachedSetlistsAt: {previous}; New: {new}", existing.CachedSetlistsAt, concert.CachedSetlistsAt);
            if (concert.CachedSetlistsAt == null || concert.CachedSetlistsAt < existing.CachedSetlistsAt)
            {
                _logger.LogDebug("Discard the cached setlist fields from the request as the stored data is more recent.");
                concert.CachedSetlistsJson = existing.CachedSetlistsJson;
                concert.CachedSetlistsAt = existing.CachedSetlistsAt;
            }
            else
            {
                _logger.LogDebug("Not overriding setlist fields because the new data is more recent.");
            }
        }
        
        concert.LastChange = DateTimeOffset.UtcNow;
    }
}