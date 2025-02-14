using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Lambda.GetS3UploadUrl.UploadRequester;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.GetS3UploadUrl;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();
    private readonly IAmazonS3 _s3Client;
    
    public Function() : this(new AmazonS3Client())
    {
    }
    
    
    internal Function(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        if (request.Body == null)
        {
            return new APIGatewayProxyResponse
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

        BaseUploadRequester requester = GetRequester();

        GetS3UploadUrlResponse response = new GetS3UploadUrlResponse
        {
            UploadUrl = requester.GetUploadUrl()
        };
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(response),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }
    
    
    private GetS3UploadUrlRequest GetRequestObjectFromJsonBody(string json)
    {
        return JsonSerializer.Deserialize<GetS3UploadUrlRequest>(json) ?? throw new InvalidDataContractException("JSON could not be parsed to Concert!");
    }


    private BaseUploadRequester GetRequester()
    {
        return null;
    }


    private string GetUploadUrl(string bucketName, string contentType, string concertId, string subDir, string fileExtension, TimeSpan? validity = null)
    {
        var fileId = Guid.NewGuid().ToString();
        return _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest()
        {
            BucketName = bucketName,
            Key = $"{concertId}/{subDir}/{fileId}.{fileExtension}",
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(validity ?? TimeSpan.FromMinutes(15))
        });
    }
}