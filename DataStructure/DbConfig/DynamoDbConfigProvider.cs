using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure.DbConfig;

public class DynamoDbConfigProvider
{
    public enum Table
    {
        Concerts,
        ConcertBookmarks,
        AuditLog
    }
    
    /// <summary>
    /// Fetches the table name for the <paramref name="table"/> from the env variables
    /// </summary>
    /// <param name="table">Table to get the config for</param>
    /// <returns>SaveConfig for DynamoDB</returns>
    public SaveConfig GetSaveConfigFor(Table table)
    {
        var tableName = GetTableNameFor(table);
        if (tableName == null)
        {
            return new SaveConfig();
        }

        return new SaveConfig
        {
            OverrideTableName = tableName
        };
    }

    private static string? GetTableNameFor(Table table)
    {
        return table switch
        {
            Table.Concerts => Environment.GetEnvironmentVariable("CONCERTS_TABLE_NAME"),
            Table.ConcertBookmarks => Environment.GetEnvironmentVariable("CONCERT_BOOKMARKS_TABLE_NAME"),
            Table.AuditLog => Environment.GetEnvironmentVariable("AUDIT_LOG_TABLE_NAME"),
            _ => null
        };
    }
}