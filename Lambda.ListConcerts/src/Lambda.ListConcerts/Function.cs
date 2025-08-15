using System.Globalization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ListConcerts;

using DateRange = (DateTimeOffset? from, DateTimeOffset? to);

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();

    public Function()
    {
        AmazonDynamoDBConfig config = new AmazonDynamoDBConfig
        {
            LogMetrics = true,
            LogResponse = true
        };

        _dynamoDbClient = new AmazonDynamoDBClient(config);

        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        //_dynamoDbContext.RegisterCustomConverters();
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Path: {request.Path}");
        context.Logger.LogInformation($"Request: {JsonSerializer.Serialize(request)}");
        
        if (request.QueryStringParameters != null)
        {
            var idParamFound = request.QueryStringParameters.TryGetValue("id", out var searchId);
            if (idParamFound && searchId != null)
            {
                // Find one concert
                return await ReturnSingleConcert(searchId);
            }
            
            // check date range
            var dateFromFound = request.QueryStringParameters.TryGetValue("date_from", out var dateFromStr);
            var dateToFound = request.QueryStringParameters.TryGetValue("date_to", out var dateToStr);

            var onlyFuture = !dateFromFound && !dateToFound;
            
            var filterTourParameterFound = request.QueryStringParameters.TryGetValue("tour", out var filterTourStr);
            if (filterTourParameterFound || dateFromFound  || dateToFound)
            {
                var dateRangeFilter = GetDateRangeFrom(dateFromStr, dateToStr, context.Logger);

                return await ReturnFilteredConcertList(context, filterTourStr, dateRangeFilter);
            }

            // no filters were used. Return all concerts
            context.Logger.LogDebug("Content of only_future variable: {value}", onlyFuture);
            return await ReturnAllConcerts(context, onlyFuture);
        }
        
        if (request.Path == "/concerts/next")
        {
            context.Logger.LogInformation("Requested next concert");
            return await ReturnNextConcert(context);
        }
        
        if (request.PathParameters != null && request.PathParameters.TryGetValue("id", out var idParameter))
        {
            context.Logger.LogInformation("Requested ID: {id}", idParameter);
            if (idParameter == "next")
            {
                return await ReturnNextConcert(context);
            }

            return await ReturnSingleConcert(idParameter);
        }
        
        // List all concerts
        context.Logger.LogDebug("Fallback to return all FUTURE concerts");
        return await ReturnAllConcerts(context);
    }


    private async Task<APIGatewayProxyResponse> ReturnFilteredConcertList(ILambdaContext context, string? filterTour = null, DateRange? dateRange = null)
    {
        context.Logger.LogDebug("Query filtered concerts: DateRange ({from} to {to}); Tour: {tour}", dateRange?.from.ToString() ?? "null", dateRange?.to.ToString() ?? "null", filterTour);
        var searchStartDate = dateRange?.from ?? DateTimeOffset.Now;
        if (dateRange?.from == null && dateRange?.to != null)
        {
            context.Logger.LogDebug("date_from is not set, but date_to is. Will search for historic shows as well", dateRange);
            searchStartDate = DateTimeOffset.MinValue;
        }
        
        // if no end is specified, use max value to search for all shows
        var searchEndDate = dateRange?.to ?? DateTimeOffset.MaxValue;
        
        var searchStartDateStr = searchStartDate.ToString("O");
        var searchEndDateStr = searchEndDate.ToString("O");
        context.Logger.LogInformation("SCAN filtered concerts between {start} and {end}", searchStartDateStr, searchEndDateStr);

        var config = _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName();
        config.BackwardQuery = false;
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        // build Scan Conditions
        List<ScanCondition> conditions =
        [
            new("Status", ScanOperator.Equal, "PUBLISHED"),
            new("PostedStartTime", ScanOperator.Between, searchStartDate,  searchEndDate),
        ];

        if (filterTour != null)
        {
            context.Logger.LogDebug("Add filter for TourName = '{tourName}'",  filterTour);
            if (string.IsNullOrEmpty(filterTour))
            {
                context.Logger.LogDebug("Filter for shows without tour");
                conditions.Add(new ScanCondition("TourName", ScanOperator.IsNull));
            }
            else
            {
                conditions.Add(new ScanCondition("TourName", ScanOperator.Equal, filterTour));
            }
        }
        
        var query = _dynamoDbContext.ScanAsync<Concert>(conditions, config);

        var concerts = await query.GetRemainingAsync() ?? [];
        
        var concertJson = JsonSerializer.Serialize(concerts);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = concertJson,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }


    private async Task<APIGatewayProxyResponse> ReturnNextConcert(ILambdaContext context)
    {
        var now = DateTimeOffset.Now;
        var dateNowStr = now.ToString("O");
        
        context.Logger.LogInformation("Query concerts after: {time}", dateNowStr);

        var config = _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName();
        config.BackwardQuery = false;
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            "PUBLISHED", // PartitionKey value
            QueryOperator.GreaterThanOrEqual,
            [new AttributeValue { S = dateNowStr }],
            config);

        var concerts = await query.GetRemainingAsync();
        if (concerts == null || concerts.Count == 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body = "{\"message\": \"No concerts found.\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(concerts.First()),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }


    private async Task<APIGatewayProxyResponse> ReturnSingleConcert(string id)
    {
        var concert = await _dynamoDbContext.LoadAsync<Concert>(id, _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName());
        var concertJson = JsonSerializer.Serialize(concert);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = concertJson
        };
    }
    
    
    private async Task<APIGatewayProxyResponse> ReturnAllConcerts(ILambdaContext context, bool onlyFuture = true)
    {
        var searchStartDate = onlyFuture ? DateTimeOffset.Now : DateTimeOffset.MinValue;
        var searchStartDateStr = searchStartDate.ToString("O");
            
        context.Logger.LogInformation("Query concerts after: {time}", searchStartDateStr);

        var config = _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName();
        config.BackwardQuery = false;
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            "PUBLISHED", // PartitionKey value
            QueryOperator.GreaterThanOrEqual,
            [new AttributeValue { S = searchStartDateStr }],
            config);

        var concerts = await query.GetRemainingAsync() ?? [];
        
        var concertJson = JsonSerializer.Serialize(concerts);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = concertJson,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }


    /// <summary>
    /// Returns a date range as tuple in UTC. The inputs are allowed to be any timezone as long as the offset is specified
    /// </summary>
    /// <param name="fromStr">ISO String</param>
    /// <param name="toStr">ISO String</param>
    /// <param name="logger">Logger</param>
    /// <returns>tuple of DateTimeOffsets</returns>
    private static DateRange GetDateRangeFrom(string? fromStr, string? toStr, ILambdaLogger logger)
    {
        logger.LogDebug("GetDateRangeFrom: {from}, {to}", fromStr, toStr);
        var fromParsed = DateTimeOffset.TryParse(fromStr ?? "", out var dateFrom);
        var toParsed = DateTimeOffset.TryParse(toStr ?? "", out var dateTo);
        
        // make sure to use UTC time
        var dateRange = new DateRange(fromParsed ? dateFrom.ToOffset(TimeSpan.Zero) : null, toParsed ? dateTo.ToOffset(TimeSpan.Zero) : null);
        logger.LogDebug("Parsed from: {from}: Parsed to: {to}", fromParsed, toParsed);
        return dateRange;
    }
}