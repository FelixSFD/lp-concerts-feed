using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Utils.Cache;
using Database.Concerts;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.CalendarFeed;

public class Function
{
    private const string QueryParamEventCatFlags = "event_categories";
    
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private static AmazonS3Client _s3Client = new();
    private readonly ICalendarGenerator _calendarGenerator;
    private readonly string _calendarCacheBucketName;

    
    public Function()
    {
        _calendarCacheBucketName = Environment.GetEnvironmentVariable("ICAL_BUCKET_ARN") ?? throw new Exception("S3 bucket not configured in environment variable");
        _calendarGenerator = new CalendarGenerator();
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        var concertRepository = new DynamoDbConcertRepository(_dynamoDbContext, _dbConfigProvider, context.Logger);
        
        // Source to cancel the request after timeout
        using var cts = new CancellationTokenSource(context.RemainingTime);

        // even though it's not marked as nullable, QueryStringParameters can be null on runtime in some cases
        IDictionary<string, string> safeQueryStringParams = request.QueryStringParameters ?? new Dictionary<string, string>();
        _ = safeQueryStringParams.TryGetValue(QueryParamEventCatFlags, out string? eventCategoriesFlags);
        ConcertSubEventCategory eventCategories;
        if (eventCategoriesFlags != null)
        {
            context.Logger.LogInformation("Requested event categories: {cat}", eventCategoriesFlags);
            eventCategories = GetCategoryFlagsFromQueryParam(eventCategoriesFlags);
        }
        else
        {
            context.Logger.LogInformation("Requested default categories.");
            eventCategories = ConcertSubEventCategory.LinkinPark;
        }
        
        var fileName = CalendarHelper.GetFileNameFor(eventCategories);
        context.Logger.LogDebug("Calculated filename for the iCal: {fileName}", fileName);

        var concerts = concertRepository.GetConcertsAsync(dateRange: (DateTimeOffset.MinValue, DateTimeOffset.MaxValue));
        var calendar = new Calendar();
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin")); // TODO: Get correct timezone
        DateTimeOffset? latestChange = DateTimeOffset.MinValue;
        await foreach (var concert in concerts)
        {
            calendar.Events.AddRange(concert.ToCalendarEvents(eventCategories));
            
            if ((concert.LastChange ?? DateTimeOffset.MinValue) > latestChange)
                latestChange = concert.LastChange;
            
            if ((concert.DeletedAt ?? DateTimeOffset.MinValue) > latestChange)
                latestChange = concert.DeletedAt;
        }
        
        context.Logger.LogDebug("Latest change was at: {latestChange}", latestChange);
        context.Logger.LogDebug("Generated {i} events.", calendar.Events.Count);
        
        var serializedCalendar = SerializeCalendar(calendar, context);
        if (!string.IsNullOrEmpty(serializedCalendar))
        {
            await StoreInCache(eventCategories, serializedCalendar!, DateTimeOffset.Now, context.Logger);
        }
        
        return new APIGatewayProxyResponse()
        {
            StatusCode = 200,
            Body = serializedCalendar,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/calendar" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" },
                { "Cache-Control", CacheControlHeaderFactory.CacheFor(CacheExpiration.Medium) }
            }
        };
    }


    private async Task StoreInCache(ConcertSubEventCategory eventCategories, string serializedCalendar, DateTimeOffset latestChange, ILambdaLogger logger)
    {
        logger.LogDebug("Storing calendar in cache with latest change at: {latestChange}", latestChange);
        logger.LogDebug("Bucket ARN: {bucket}", _calendarCacheBucketName);
        var fileName = CalendarHelper.GetFileNameFor(eventCategories);
        var putRequest = new PutObjectRequest
        {
            BucketName = _calendarCacheBucketName,
            Key = fileName,
            ContentBody = serializedCalendar,
            ContentType = "text/calendar"
        };
        putRequest.Metadata.Add("concert-data-version", latestChange.ToString("O"));

        try
        {
            var putResponse = await _s3Client.PutObjectAsync(putRequest);
            logger.LogDebug("Successfully stored calendar in cache. Status code: {statusCode}",
                putResponse.HttpStatusCode);
        }
        catch (AmazonS3Exception e)
        {
            logger.LogError(e, "Error storing calendar in cache.");
        }
    }


    /*private async Task<bool> HasLatestFileInS3(string fileName)
    {
        var getDataVersionObjectRequest = new GetObjectRequest
        {
            BucketName = _calendarCacheBucketName,
            Key = $"{fileName}.data-timestamp"
        };

        var getDataVersionObjectResponse = await _s3Client.GetObjectAsync(getDataVersionObjectRequest);
        getDataVersionObjectResponse.
    }*/

    private string? SerializeCalendar(Calendar calendar, ILambdaContext context)
    {
        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(calendar);
        return serializedCalendar;
    }


    /// <summary>
    /// Reads a param string with flags to a combined <see cref="ConcertSubEventCategory"/>
    /// </summary>
    /// <param name="param">comma-separated string of enum options</param>
    /// <returns></returns>
    private ConcertSubEventCategory GetCategoryFlagsFromQueryParam(string param)
    {
        var eventCategories = param.Split(',')
            .Select(catStr =>
            {
                _ = Enum.TryParse(catStr, out ConcertSubEventCategory flag);
                return flag;
            });

        return eventCategories
            .Aggregate<ConcertSubEventCategory, ConcertSubEventCategory>(default, (current, flag) => current | flag);
    }
}