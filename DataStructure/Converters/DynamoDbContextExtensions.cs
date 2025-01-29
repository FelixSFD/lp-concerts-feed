using Amazon.DynamoDBv2.DataModel;

namespace LPCalendar.DataStructure.Converters;

public static class DynamoDbContextExtensions
{
    public static void RegisterCustomConverters(this DynamoDBContext context)
    {
        context.ConverterCache.Add(typeof(DateTimeOffset), new DateTimeOffsetToStringPropertyConverter());
        context.ConverterCache.Add(typeof(DateTimeOffset?), new DateTimeOffsetToStringPropertyConverter());
    }
}