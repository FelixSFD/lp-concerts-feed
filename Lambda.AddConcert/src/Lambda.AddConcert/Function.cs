using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.Responses;
using ErrorResponse = LPCalendar.DataStructure.Responses.ErrorResponse;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AddConcert;

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
        
        context.Logger.LogInformation("Validate request");
        bool isValid = RequestIsValid(concert, out var errors);
        if (!isValid)
        {
            context.Logger.LogWarning("Request was not valid. Will return 400");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonSerializer.Serialize(errors),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
                }
            };
        }
        
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


    private bool RequestIsValid(Concert concert, [NotNullWhen(true)] out InvalidFieldsErrorResponse? errors)
    {
        bool valid = true;
        var tmpResponse = new InvalidFieldsErrorResponse
        {
            Message = "Some fields are not valid"
        };

        if (concert.LpuEarlyEntryTime.HasValue && !concert.LpuEarlyEntryConfirmed)
        {
            valid = false;
            tmpResponse.AddInvalidField(nameof(concert.LpuEarlyEntryTime), $"LPU Early Entry time can only be set, if {concert.LpuEarlyEntryConfirmed} is set to true");
        }

        errors = valid ? null : tmpResponse;
        return valid;
    }


    private Concert MakeConcertFromJsonBody(string json)
    {
        string guid = Guid.NewGuid().ToString();
        Concert concert = JsonSerializer.Deserialize<Concert>(json) ?? throw new InvalidDataContractException("JSON could not be parsed to Concert!");
        concert.Id = Guid.TryParse(concert.Id, out _) ? concert.Id : guid;
        return concert;
    }


    private async Task FixNonOverridableFields(Concert concert)
    {
        var existing = await _dynamoDbContext.LoadAsync<Concert>(concert.Id, _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName());
        if (existing != null)
        {
            concert.ScheduleImageFile = existing.ScheduleImageFile;
        }
    }


    private async Task SaveConcert(Concert concert)
    {
        await FixNonOverridableFields(concert);
        await _dynamoDbContext.SaveAsync(concert, _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName());
    }
}