using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

// disable nullable here, since IPropertyConverter doesn't seem to use it properly
#nullable disable

namespace LPCalendar.DataStructure.Converters;

public class DateTimeToStringPropertyConverter : IPropertyConverter
{
    public object FromEntry(DynamoDBEntry entry)
    {
        var dateTime = entry?.AsString();
        if (string.IsNullOrEmpty(dateTime))
            return null;
        if (!DateTime.TryParse(dateTime, out DateTime value))
            throw new ArgumentException("entry parameter must be a validate DateTime value.", nameof(entry));
        else
            return value;
    }
    public DynamoDBEntry ToEntry(object value)
    {
        if (value == null)
            return new DynamoDBNull();

        if (value.GetType() == typeof(AttributeValue))
        {
            var dateStr = ((AttributeValue)value).S;
            return dateStr;
        }
        
        if (value.GetType() != typeof(DateTime) && value.GetType() != typeof(DateTime?))
            throw new ArgumentException($"value parameter must be a DateTime or a Nullable<DateTime>. But it is '{value.GetType()}'!", nameof(value));
        
        return ((DateTime)value).ToString("O");
    }
}