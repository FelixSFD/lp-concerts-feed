using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace LPCalendar.DataStructure.Converters;

public class DateTimeOffsetToStringPropertyConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        var dateTimeValue = value as DateTimeOffset?;
        var dateTimeString = dateTimeValue?.ToString("o");
        var entry = new Primitive
        {
            Value = dateTimeString,
        };

        return entry;
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        var primitive = entry as Primitive;
        if (primitive is not { Value: string dateTimeString })
        {
            throw new DateTimeConverterException("DB entry is not a string!");
        }

        var dateTimeValue = DateTimeOffset.Parse(dateTimeString);

        return dateTimeValue;
    }
}