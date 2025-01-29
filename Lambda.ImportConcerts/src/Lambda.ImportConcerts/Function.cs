using System.Web;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ImportConcerts;

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
    
    
    public async Task<string> FunctionHandler(S3Event evt, ILambdaContext context)
    {
        try
        {
            if (evt.Records.Count <= 0)
            {
                context.Logger.LogError("Empty S3 Event received");
                return string.Empty;
            }

            var bucket = evt.Records[0].S3.Bucket.Name;
            var key = HttpUtility.UrlDecode(evt.Records[0].S3.Object.Key);

            context.Logger.LogInformation($"Request is for {bucket} and {key}");

            var objectResult = await _s3Client.GetObjectAsync(bucket, key);

            context.Logger.LogInformation($"Returning {objectResult.Key}");

            TextReader tr = new StreamReader(objectResult.ResponseStream);
            var csvContent = await tr.ReadToEndAsync();

            var count = 0;
            foreach (var concert in CsvToConcertsConverter.GetConcerts(csvContent))
            {
                context.Logger.LogInformation("Importing concert with ID {0}", concert.Id);
                await SaveConcert(concert);
                count++;
            }

            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = objectResult.BucketName,
                Key = objectResult.Key
            });

            return $"Imported {count} concerts.";
        }
        catch (Exception e)
        {
            context.Logger.LogError($"Error processing request - {e.Message}");

            return string.Empty;
        }
    }
    
    
    private async Task SaveConcert(Concert concert)
    {
        await _dynamoDbContext.SaveAsync(concert, _dbOperationConfigProvider.GetConcertsConfigWithEnvTableName());
    }
}