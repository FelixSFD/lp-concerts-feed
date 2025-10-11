using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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
    
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly ICalendarGenerator _calendarGenerator;

    
    public Function() : this(new CalendarGenerator())
    {
    }
    
    
    public Function(ICalendarGenerator calendarGenerator)
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
        _calendarGenerator = calendarGenerator;
    }
    
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
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
        
        var calendar = new Ical.Net.Calendar();
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin")); // TODO: Get correct timezone
        
        var concerts = await _dynamoDbContext
            .ScanAsync<Concert>(new List<ScanCondition>(), _dbConfigProvider.GetScanConfigFor(DynamoDbConfigProvider.Table.Concerts))
            .GetRemainingAsync(cts.Token);
        
        calendar.Events.AddRange(concerts.ToCalendarEvents(eventCategories));
        context.Logger.LogDebug("Generated {i} events.", calendar.Events.Count);
        
        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(calendar);
        
        return new APIGatewayProxyResponse()
        {
            StatusCode = 200,
            Body = serializedCalendar,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/calendar" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
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