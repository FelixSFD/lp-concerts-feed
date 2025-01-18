using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure;

public class DBOperationConfigProvider
{
    public DynamoDBOperationConfig GetConcertsConfigWithEnvTableName()
    {
        return new DynamoDBOperationConfig
        {
            OverrideTableName = Environment.GetEnvironmentVariable("CONCERTS_TABLE_NAME")
        };
    }
}