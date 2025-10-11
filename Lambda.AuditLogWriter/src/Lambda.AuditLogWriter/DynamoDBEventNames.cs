namespace Lambda.AuditLogWriter;

public static class DynamoDBEventNames
{
    public const string Insert = "INSERT";
    public const string Modify = "MODIFY";
    public const string Remove = "REMOVE";
}