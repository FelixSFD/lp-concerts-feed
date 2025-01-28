using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace LPCalendar.DataStructure.Converters;

public class DateTimeOffsetToStringPropertyConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        var dateTimeValue = value as DateTimeOffset?;
        if (dateTimeValue == null)
        {
            throw new ArgumentOutOfRangeException();
        }

        var dateTimeString = dateTimeValue.Value.ToString("o");
        var entry = new Primitive
        {
            Value = dateTimeString,
        };

        return entry;
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        var primitive = entry as Primitive;
        if (primitive is not { Value: string dateTimeString } || string.IsNullOrEmpty(dateTimeString))
        {
            throw new ArgumentOutOfRangeException();
        }

        var dateTimeValue = DateTimeOffset.Parse(dateTimeString);

        return dateTimeValue;
    }
}