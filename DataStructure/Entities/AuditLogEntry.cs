using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using LPCalendar.DataStructure.Converters;

namespace LPCalendar.DataStructure.Entities;

[DynamoDBTable(AuditLogTableName)]
public class AuditLogEntry
{
    public const string AuditLogTableName = "AuditLogV1";
    
    /// <summary>
    /// ID of the user who performed the action
    /// </summary>
    [DynamoDBHashKey]
    [DynamoDBGlobalSecondaryIndexHashKey("UserHistoryIndex")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    
    /// <summary>
    /// Action the user performed
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("action")]
    public string Action { get; set; }
    
    /// <summary>
    /// If applicable, a description if the entity that was modified
    /// </summary>
    [DynamoDBProperty]
    [DynamoDBGlobalSecondaryIndexHashKey("EntityHistoryIndex")]
    [JsonPropertyName("entity")]
    public string? AffectedEntity { get; set; }
    
    /// <summary>
    /// Timestamp of the action
    /// </summary>
    [DynamoDBRangeKey(typeof(DateTimeToStringPropertyConverter))]
    [DynamoDBGlobalSecondaryIndexRangeKey("UserHistoryIndex", "EntityHistoryIndex")]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Old value (if applicable)
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }
    
    /// <summary>
    /// New value (if applicable)
    /// </summary>
    [DynamoDBProperty]
    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}