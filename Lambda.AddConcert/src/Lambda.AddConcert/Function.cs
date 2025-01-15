using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AddConcert;

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
        if (request.Body == null)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{\"message\": \"Request body not found\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
                }
            };
        }
        
        // Parse JSON
        context.Logger.LogInformation("Start parsing JSON...");
        var concert = MakeConcertFromJsonBody(request.Body);
        context.Logger.LogInformation("Start writing to DB...");
        await SaveConcert(concert);
        context.Logger.LogInformation("Concert written to DB");
        
        var response = new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.Created,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };

        return response;
    }


    private Concert MakeConcertFromJsonBody(string json)
    {
        string guid = Guid.NewGuid().ToString();
        Concert concert = JsonSerializer.Deserialize<Concert>(json) ?? throw new InvalidDataContractException("JSON could not be parsed to Concert!");
        concert.Id = guid;
        return concert;
    }


    private async Task SaveConcert(Concert concert)
    {
        await _dynamoDbContext.SaveAsync(concert);
    }
}