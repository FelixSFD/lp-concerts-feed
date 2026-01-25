using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
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
}