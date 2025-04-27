using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AttributeValue = Amazon.DynamoDBv2.Model.AttributeValue;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.OneByOneNavigator;

public class Function
{
    private readonly string _tableName = Environment.GetEnvironmentVariable("CONCERTS_TABLE_NAME")!;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    
    
    public Function()
    {
        AmazonDynamoDBConfig config = new AmazonDynamoDBConfig
        {
            LogMetrics = true,
            LogResponse = true
        };

        _dynamoDbClient = new AmazonDynamoDBClient(config);
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var foundPathParam = request.PathParameters.TryGetValue("id", out var currentId);
        if (!foundPathParam && currentId != null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "ID not set",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        
        context.Logger.LogInformation($"Found ID: {currentId}");
        
        IDictionary<string, string?> results = new Dictionary<string, string?>();
        results["current"] = currentId!;
        
        // Load information about the current concert first. We need this for the next queries
        var currentConcert = await GetConcertById(currentId!);
        
        // Build the key where the searches will start
        var startKey = new Dictionary<string, AttributeValue>
        {
            ["Status"] = currentConcert["Status"],
            ["PostedStartTime"] = currentConcert["PostedStartTime"]
        };

        context.Logger.LogInformation($"Path: {request.Path}");
        context.Logger.LogInformation($"Request: {JsonSerializer.Serialize(request)}");

        // query adjacent IDs
        var nextIdTask = GetAdjacentId(startKey, true, context.Logger);
        var previousIdTask = GetAdjacentId(startKey, false, context.Logger);
        
        await Task.WhenAll(nextIdTask, previousIdTask);
        results["next"] = nextIdTask.Result;
        results["previous"] = previousIdTask.Result;

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(results),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }


    /**
     * Returns either the next or the previous ID based on the key of the current concert
     */
    private async Task<string?> GetAdjacentId(Dictionary<string, AttributeValue> startKey, bool next, ILambdaLogger logger)
    {
        logger.LogInformation($"Start key: {JsonSerializer.Serialize(startKey)}");
        var keyConditions = new Dictionary<string, Condition>
        {
            ["PostedStartTime"] = new()
            {
                AttributeValueList = [startKey["PostedStartTime"]],
                ComparisonOperator = next ? ComparisonOperator.GE : ComparisonOperator.LE
            },
            ["Status"] = new()
            {
                ComparisonOperator = ComparisonOperator.EQ,
                AttributeValueList = [new AttributeValue("PUBLISHED")]
            }
        };
        
        logger.LogInformation($"key conditions: {JsonSerializer.Serialize(keyConditions)}");

        var queryRequest = new QueryRequest
        {
            TableName = _tableName,
            IndexName = "PostedStartTimeGlobalIndex",
            Limit = 2,
            KeyConditions = keyConditions,
            ScanIndexForward = next
        };

        var query = await _dynamoDbClient.QueryAsync(queryRequest);
        if (query.Count < 2)
        {
            // no ID found as next
            return null;
        }
        else
        {
            // last item in the list will be the next ID
            var attributeValues = query.Items.Last();
            logger.LogInformation($"Found attributes: {JsonSerializer.Serialize(attributeValues)}");
            return attributeValues?["Id"].S;
        }
    }


    /**
     * Searches for a Concert by its ID
     */
    private async Task<Dictionary<string, AttributeValue>> GetConcertById(string id)
    {
        var getItemRequest = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = id } },
            }
        };

        var response = await _dynamoDbClient.GetItemAsync(getItemRequest);
        return response.Item;
    }
}