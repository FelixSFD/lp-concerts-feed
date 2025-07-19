using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
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
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();

    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.CanDeleteConcerts())
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }
        
        var response = new APIGatewayProxyResponse
        {
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST, DELETE" }
            }
        };
        
        if (!request.PathParameters.ContainsKey("concertId"))
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            return response;
        }

        var concertId = request.PathParameters["concertId"]!;

        DeleteConcertRequest deleteRequest;
        try
        {
            deleteRequest = new DeleteConcertRequest
            {
                ConcertId = concertId
            };
        }
        catch (Exception e)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Body = $"{{\"message\": \"Failed to parse request: {e.GetType().Name} - {e.Message}\"}}";
            return response;
        }

        await _dynamoDbContext.DeleteAsync<Concert>(deleteRequest.ConcertId, _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName());

        response.StatusCode = (int)HttpStatusCode.NoContent;

        return response;
    }


    private static DeleteConcertRequest? GetRequestFromJson(string json)
    {
        return JsonSerializer.Deserialize<DeleteConcertRequest>(json);
    }
}