using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime.Internal.Util;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Common.SNS;
using Lambda.Push.Sender;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.Push.EndpointCleanup;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly string _platformApplicationArn;
    private readonly Dictionary<string, bool> _endpointStatusCache = new();
    
    public Function()
    {
        _platformApplicationArn = Environment.GetEnvironmentVariable("PLATFORM_APPLICATION_ARN")
                                  ?? throw new Exception("Missing environment variable PLATFORM_APPLICATION_ARN");
        
        _sns = new AmazonSimpleNotificationServiceClient();
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        await RemoveDisabledEndpointsFromSns(context.Logger);
        await RemoveMissingEndpointsFromDb(context.Logger);
    }


    private async Task RemoveDisabledEndpointsFromSns(ILambdaLogger logger)
    {
        logger.LogDebug("Removing disabled endpoints...");
        var deleteCounter = 0;
        await foreach (var endpoint in _sns.FindAllEndpointsByPlatformApplicationAsync(_platformApplicationArn))
        {
            var enabled = endpoint.IsEnabled();
            var endpointArn = endpoint.EndpointArn;
            logger.LogDebug("Found endpoint: {endpointArn}; Enabled: {enabled}", endpointArn, enabled);
            _endpointStatusCache[endpointArn] = enabled;

            if (enabled) continue;
            
            logger.LogDebug("Remove disabled endpoint: {endpointArn}", endpointArn);
            var deleteRequest = new DeleteEndpointRequest
            {
                EndpointArn = endpointArn
            };

            await _sns.DeleteEndpointAsync(deleteRequest);
            deleteCounter++;
        }
        
        logger.LogInformation("Removed {deleteCounter} endpoints from SNS.", deleteCounter);
    }


    private async Task RemoveMissingEndpointsFromDb(ILambdaLogger logger)
    {
        logger.LogDebug("Removing endpoints from DB that are not found in SNS...");
        var deleteConfig = _dbConfigProvider.GetDeleteConfigFor(DynamoDbConfigProvider.Table.NotificationRegistrations);
        var scanCondition = new ScanCondition(nameof(NotificationUserEndpoint.EndpointArn), ScanOperator.IsNotNull);

        var deleteCounter = 0;
        IAsyncSearch<NotificationUserEndpoint> scanResult;
        do
        {
            scanResult = _dynamoDbContext.ScanAsync<NotificationUserEndpoint>(
                [scanCondition],
                _dbConfigProvider.GetScanConfigFor(DynamoDbConfigProvider.Table.NotificationRegistrations));

            foreach (var userEndpoint in await scanResult.GetNextSetAsync())
            {
                var endpointArn = userEndpoint.EndpointArn;
                var endpointEnabled = await IsEndpointEnabled(userEndpoint.EndpointArn, logger);
                logger.LogDebug("is {endpointArn} enabled? {endpointEnabled}", endpointArn, endpointEnabled);

                if (endpointEnabled) continue;
                
                logger.LogDebug("Will delete endpoint from DB: {endpointArn}", userEndpoint.EndpointArn);
                await _dynamoDbContext.DeleteAsync(userEndpoint, deleteConfig);
                deleteCounter++;
            }
        } while (!scanResult.IsDone);
        
        logger.LogInformation("Removed {deleteCounter} endpoints from DB.", deleteCounter);
    }
    
    
    /// <summary>
    /// Checks if the endpoint is enabled. Uses a cache to avoid rate limits.
    /// </summary>
    /// <param name="endpointArn"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private async Task<bool> IsEndpointEnabled(string endpointArn, ILambdaLogger logger)
    {
        if (_endpointStatusCache.TryGetValue(endpointArn, out var cachedEnabledStatus))
        {
            return cachedEnabledStatus;
        }

        var request = new GetEndpointAttributesRequest
        {
            EndpointArn = endpointArn
        };

        bool enabled;
        try
        {
            var response = await _sns.GetEndpointAttributesAsync(request);
            enabled = response.IsEnabled();
        }
        catch (NotFoundException e)
        {
            logger.LogInformation(e, "Endpoint not found: {endpointArn}", endpointArn);
            enabled = false;
        }
        
        _endpointStatusCache[endpointArn] = enabled;
        return enabled;
    }
}