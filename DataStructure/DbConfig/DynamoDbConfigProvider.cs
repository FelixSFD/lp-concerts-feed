using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure.DbConfig;

public class DynamoDbConfigProvider
{
    public enum Table
    {
        Concerts,
        ConcertBookmarks,
        AuditLog,
        NotificationRegistrations
    }
    
    
    public SaveConfig GetSaveConfigFor(Table table)
    {
        var config = new SaveConfig();
        SetTableNameOverride(config, table);
        return config;
    }
    
    
    public DeleteConfig GetDeleteConfigFor(Table table)
    {
        var config = new DeleteConfig();
        SetTableNameOverride(config, table);
        return config;
    }
    
    
    public LoadConfig GetLoadConfigFor(Table table)
    {
        var config = new LoadConfig();
        SetTableNameOverride(config, table);
        return config;
    }
    
    
    public ScanConfig GetScanConfigFor(Table table)
    {
        var config = new ScanConfig();
        SetTableNameOverride(config, table);
        return config;
    }
    
    
    public QueryConfig GetQueryConfigFor(Table table)
    {
        var config = new QueryConfig();
        SetTableNameOverride(config, table);
        return config;
    }


    /// <summary>
    /// Sets the <see cref="BaseOperationConfig.OverrideTableName"/> of the config, if the table is defined
    /// </summary>
    /// <param name="config"></param>
    /// <param name="table"></param>
    private static void SetTableNameOverride(BaseOperationConfig config, Table table)
    {
        var tableName = GetTableNameFor(table);
        if (tableName != null)
        {
            config.OverrideTableName = tableName;
        }
    }
    

    /// <summary>
    /// Fetches the table name for the <paramref name="table"/> from the env variables
    /// </summary>
    /// <param name="table">Table to get the config for</param>
    /// <returns>SaveConfig for DynamoDB</returns>
    private static string? GetTableNameFor(Table table)
    {
        return table switch
        {
            Table.Concerts => Environment.GetEnvironmentVariable("CONCERTS_TABLE_NAME"),
            Table.ConcertBookmarks => Environment.GetEnvironmentVariable("CONCERT_BOOKMARKS_TABLE_NAME"),
            Table.AuditLog => Environment.GetEnvironmentVariable("AUDIT_LOG_TABLE_NAME"),
            Table.NotificationRegistrations => Environment.GetEnvironmentVariable("NOTIFICATION_REGISTRATIONS_TABLE_NAME"),
            _ => null
        };
    }
}