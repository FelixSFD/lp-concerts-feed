using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Requests;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.DeleteConcert;

/// <summary>
/// AWS Lambda function to delete a concert from the database
/// </summary>
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
        var response = new APIGatewayProxyResponse
        {
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST, DELETE" }
            }
        };

        DeleteConcertRequest deleteRequest;
        try
        {
            deleteRequest = GetRequestFromJson(request.Body) ??
                            throw new JsonException("Failed to parse JSON request. Parser returned null.");
        }
        catch (Exception e)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Body = $"{{\"message\": \"Failed to parse request: {e.GetType().Name} - {e.Message}\"}}";
            return response;
        }

        await _dynamoDbContext.DeleteAsync<Concert>(deleteRequest.ConcertId);

        response.StatusCode = (int)HttpStatusCode.NoContent;

        return response;
    }


    private static DeleteConcertRequest? GetRequestFromJson(string json)
    {
        return JsonSerializer.Deserialize<DeleteConcertRequest>(json);
    }
}