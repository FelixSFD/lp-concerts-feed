using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure.Converters;

public static class DynamoDbContextExtensions
{
    public static void RegisterCustomConverters(this DynamoDBContext context)
    {
        _ = context.ConverterCache.TryAdd(typeof(DateTimeOffset), new DateTimeOffsetToStringPropertyConverter());
        _ = context.ConverterCache.TryAdd(typeof(DateTimeOffset?), new DateTimeOffsetToStringPropertyConverter());
    }
}