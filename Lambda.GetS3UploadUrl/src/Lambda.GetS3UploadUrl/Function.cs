using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Lambda.Auth;
using Lambda.GetS3UploadUrl.UploadRequester;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.GetS3UploadUrl;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonS3 _s3Client;
    private readonly IGuidService _guidService;
    
    public Function() : this(new AmazonS3Client(), new GuidService())
    {
    }
    
    
    internal Function(IAmazonS3 s3Client, IGuidService guidService)
    {
        _s3Client = s3Client;
        _guidService = guidService;
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.CanUpdateConcerts())
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, PUT, POST");
        }
        
        if (request.Body == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{\"message\": \"Request body not found\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, PUT, POST" }
                }
            };
        }
        
        context.Logger.LogDebug("Requested URL: {req}", request.Body);

        var uploadUrlRequest = GetRequestObjectFromJsonBody(request.Body);
        
        // init requester class
        var requester = uploadUrlRequest.Type switch
        {
            GetS3UploadUrlRequest.FileType.ConcertSchedule => new ConcertScheduleUploadRequester(uploadUrlRequest, _guidService, _s3Client),
            _ => throw new NotImplementedException($"Type '{uploadUrlRequest.Type} not implemented!")
        };

        var response = new GetS3UploadUrlResponse
        {
            UploadUrl = requester.GetUploadUrl()
        };
        
        context.Logger.LogDebug("Generated URL: {url}", response.UploadUrl);

        await requester.UpdateConcert(uploadUrlRequest.ConcertId, _dynamoDbContext, _dbConfigProvider);
        context.Logger.LogDebug("Updated concert...");
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(response),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, PUT, POST" }
            }
        };
    }
    
    
    private static GetS3UploadUrlRequest GetRequestObjectFromJsonBody(string json)
    {
        return JsonSerializer.Deserialize<GetS3UploadUrlRequest>(json) ?? throw new InvalidDataContractException("JSON could not be parsed to Concert!");
    }
}