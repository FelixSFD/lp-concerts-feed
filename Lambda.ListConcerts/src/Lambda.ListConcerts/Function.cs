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

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();

    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
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
        return await ReturnAllConcerts();
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
            [dateNowStr],
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
    
    
    private async Task<APIGatewayProxyResponse> ReturnAllConcerts()
    {
        var concertsUnsorted = await _dynamoDbContext
            .ScanAsync<Concert>(new List<ScanCondition>(),
                _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName())
            .GetRemainingAsync();

        var concerts = concertsUnsorted
            .OrderBy(c => c.PostedStartTime)
            .ToArray();
        
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
}