using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ListConcerts;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();

    private readonly string TableName;

    public Function()
    {
        TableName = _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName().OverrideTableName;
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        if (request.QueryStringParameters != null)
        {
            var idParamFound = request.QueryStringParameters.TryGetValue("id", out var searchId);
            if (idParamFound && searchId != null)
            {
                // Find one concert
                return await ReturnSingleConcert(searchId);
            }
        }
        
        if (request.PathParameters.TryGetValue("id", out var idParameter))
        {
            return await ReturnSingleConcert(idParameter);
        }

        if (request.RawPath == "/concerts/next")
        {
            return await ReturnNextConcert();
        }
        
        // List all concerts
        return await ReturnAllConcerts();
    }


    private async Task<APIGatewayProxyResponse> ReturnNextConcert()
    {
        var now = new DateTimeOffset();
        var dateNowStr = now.ToString("O");
        
        var queryConditions = new List<ScanCondition>
        {
            new("Id", ScanOperator.GreaterThanOrEqual, dateNowStr)
        };

        var config = new DynamoDBOperationConfig
        {
            BackwardQuery = false,
            IndexName = "PostedStartTimeIndex"
        };

        var concerts = await _dynamoDbContext.QueryAsync<Concert>(queryConditions, config).GetRemainingAsync();
        if (concerts.Count == 0)
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
        return new APIGatewayProxyResponse()
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
            .OrderBy(c => c.PostedStartTimeValue)
            .ToArray();
        
        var concertJson = JsonSerializer.Serialize(concerts);
        return new APIGatewayProxyResponse()
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