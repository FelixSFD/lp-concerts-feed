using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;

namespace Common.DynamoDb;

public abstract class BaseDynamoDbRepository
{
    protected readonly ILambdaLogger Logger;
    protected readonly DynamoDBContext DynamoDbContext;
    protected readonly DynamoDbConfigProvider DbConfigProvider;
    

    protected BaseDynamoDbRepository(DynamoDBContext dynamoDbContext, DynamoDbConfigProvider dbConfigProvider, ILambdaLogger logger)
    {
        Logger = logger;
        DynamoDbContext = dynamoDbContext;
        DbConfigProvider = dbConfigProvider;
        
        DynamoDbContext.RegisterCustomConverters();
    }
}