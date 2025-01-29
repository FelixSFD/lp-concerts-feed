using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

public class InvalidFieldsErrorResponse : ErrorResponse
{
    public class InvalidField
    {
        [JsonPropertyName("fieldName")]
        public required string FieldName { get; set; }
        
        [JsonPropertyName("error")]
        public required string Error { get; set; }
    }

    [JsonPropertyName("invalidFields")]
    public List<InvalidField> InvalidFields { get; } = [];


    public void AddInvalidField(string fieldName, string errorMessage)
    {
        InvalidFields.Add(new InvalidField
        {
            FieldName = fieldName,
            Error = errorMessage
        });
    }
}