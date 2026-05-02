using System.Diagnostics;
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
    
    private static AmazonS3Client _s3Client = new();
    private readonly ICalendarGenerator _calendarGenerator;
    private readonly string _calendarCacheBucketName;

    
    public Function()
    {
        _calendarCacheBucketName = Environment.GetEnvironmentVariable("ICAL_BUCKET_ARN") ?? throw new Exception("S3 bucket not configured in environment variable");
        _calendarGenerator = new CalendarGenerator();
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        var concertRepository = DynamoDbConcertRepository.CreateDefault(context.Logger);
        
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
        var calendar = new Calendar();
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin")); // TODO: Get correct timezone
        
        DateTimeOffset? latestChange = await concertRepository.GetLastChangedAsync();
        context.Logger.LogDebug("Latest change was at: {latestChange}", latestChange);

        string? serializedCalendar;
        var currentFileVersion = await GetDataVersionOfFile(fileName, context.Logger);
        if (currentFileVersion == null || latestChange > currentFileVersion)
        {
            context.Logger.LogInformation("iCal in cache is outdated or not found. Generate a new one. Current file version: {currentFileVersion}; Latest change in DB: {latestChangeInDb}", currentFileVersion, latestChange);

            var stopwatch = Stopwatch.StartNew();
            var concerts = concertRepository.GetConcertsAsync(dateRange: (DateTimeOffset.MinValue, DateTimeOffset.MaxValue));
            await foreach (var concert in concerts)
            {
                calendar.Events.AddRange(concert.ToCalendarEvents(eventCategories));
            }
            stopwatch.Stop();
            
            context.Logger.LogDebug("Generated {i} events in {duration} ms.", calendar.Events.Count, stopwatch.ElapsedMilliseconds);
            
            serializedCalendar = SerializeCalendar(calendar, context);
            context.Logger.LogDebug("Serialized new calendar object.");
            if (!string.IsNullOrEmpty(serializedCalendar))
            {
                await StoreInCache(eventCategories, serializedCalendar, latestChange ?? DateTimeOffset.MinValue, context.Logger);
            }
        }
        else
        {
            serializedCalendar = await GetCalFromCache(eventCategories, context.Logger);
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
                { "Cache-Control", CacheControlHeaderFactory.CacheFor(CacheExpiration.Medium, CacheFlags.Public) }
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
    
    
    private async Task<string> GetCalFromCache(ConcertSubEventCategory eventCategories, ILambdaLogger logger)
    {
        logger.LogDebug("Bucket ARN: {bucket}", _calendarCacheBucketName);
        var fileName = CalendarHelper.GetFileNameFor(eventCategories);
        logger.LogDebug("Retrieve from S3 bucket: {fileName}", fileName);
        var getRequest = new GetObjectRequest
        {
            BucketName = _calendarCacheBucketName,
            Key = fileName
        };
        
        try
        {
            var getResponse = await _s3Client.GetObjectAsync(getRequest);
            logger.LogDebug("Successfully retrieved calendar from cache. Status code: {statusCode}",
                getResponse.HttpStatusCode);

            using var reader = new StreamReader(getResponse.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception e)
        {
            logger.LogError(e, "Error storing calendar in cache.");
            return string.Empty;
        }
    }


    private async Task<DateTimeOffset?> GetDataVersionOfFile(string fileName, ILambdaLogger logger)
    {
        var getObjectMetadataRequest = new GetObjectMetadataRequest
        {
            BucketName = _calendarCacheBucketName, 
            Key = fileName
        };

        try
        {
            var getObjectMetadataResponse = await _s3Client.GetObjectMetadataAsync(getObjectMetadataRequest);
            var version = getObjectMetadataResponse.Metadata["concert-data-version"];
            if (string.IsNullOrEmpty(version))
            {
                logger.LogWarning("No version found for: {fileName}", fileName);
                return null;
            }

            var didParse = DateTimeOffset.TryParse(version, out var parsedVersion);
            if (!didParse)
            {
                logger.LogError("Could not parse concert-data-version: {version}", version);
            }

            return parsedVersion;
        }
        catch (AmazonS3Exception e)
        {
            logger.LogWarning(e, "Error retrieving concert-data-version from S3. Status code: {statusCode}", e.StatusCode);
            return null;
        }
    }

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