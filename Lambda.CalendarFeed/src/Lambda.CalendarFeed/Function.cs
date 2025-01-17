using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.CalendarFeed;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly IDynamoDBContext _dynamoDbContext;

    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
    }
    
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        // Source to cancel the request after timeout
        using var cts = new CancellationTokenSource(context.RemainingTime);
        
        var calendar = new Ical.Net.Calendar();
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin")); // TODO: Get correct timezone

        /*await GetItemsAsync<Concert>(Concert.ConcertTableName)
            .Select(GetCalendarEventFor)
            .Where(o => o != null)
            .Select(o => o!)
            .ForEachAsync(calendar.Events.Add, cancellationToken: cts.Token);*/
        var concerts = await _dynamoDbContext.ScanAsync<Concert>(new List<ScanCondition>()).GetRemainingAsync(cts.Token);
        calendar.Events.AddRange(concerts.Select(GetCalendarEventFor));
        var serializer = new CalendarSerializer();
        var serializedCalendar = serializer.SerializeToString(calendar);
        Console.WriteLine(serializedCalendar);
        
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


    private CalendarEvent? GetCalendarEventFor(Concert concert)
    {
        if (concert.PostedStartTimeValue == null)
            return null;
        
        var date = new CalDateTime(concert.PostedStartTimeValue.Value.DateTime, concert.TimeZoneId); // TODO: maybe extension method?
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = $"Linkin Park {concert.Venue} (TODO: Tour etc.)", // Should always be present
            Description = "Linkin Park Concert description", // optional
            Location = $"{concert.Venue}, {concert.City}, {concert.Country}",
            Start = date,
            Duration = TimeSpan.FromHours(2),
            IsAllDay = false
        };

        return calendarEvent;
    }
    
    
    private async IAsyncEnumerable<T> GetItemsAsync<T>(string tableName) where T : class
    {
        // Create a paginator for scanning the table
        var paginator = _dynamoDbClient.Paginators.Scan(new ScanRequest
        {
            TableName = tableName
        });

        await foreach (var page in paginator.Responses)
        {
            foreach (var item in page.Items)
            {
                yield return ParseItemToObject<T>(item);
            }
        }
    }


    private static T ParseItemToObject<T>(Dictionary<string, AttributeValue> attributes) where T : class
    {
        string json = JsonSerializer.Serialize(attributes);
        return JsonSerializer.Deserialize<T>(json) ?? throw new JsonException("Failed to parse database item!");
    }
}