using System.Text;
using ChoETL;
using LPCalendar.DataStructure;

namespace Lambda.ImportConcerts;

public class CsvToConcertsConverter
{
    public static IEnumerable<Concert> GetConcerts(string csvContent)
    {
        var config = new ChoCSVRecordConfiguration
        {
            QuoteAllFields = true,
            FileHeaderConfiguration =
            {
                QuoteAllHeaders = true
            },
            IgnoredFields = { "PostedStartTimeValue" },
            IgnoreFieldValueMode = ChoIgnoreFieldValueMode.Any,
            ColumnCountStrict = false,
            ObjectValidationMode = ChoObjectValidationMode.Off,
            ThrowAndStopOnMissingField = false
        };
        
        return new ChoCSVReader<Concert>(new StringBuilder(csvContent), config)
            .WithFirstLineHeader();
    }
}