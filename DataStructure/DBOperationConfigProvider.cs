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
    
    
    public DynamoDBOperationConfig GetAuditLogEntryConfigWithEnvTableName()
    {
        return new DynamoDBOperationConfig
        {
            OverrideTableName = Environment.GetEnvironmentVariable("AUDIT_LOG_TABLE_NAME")
        };
    }
}