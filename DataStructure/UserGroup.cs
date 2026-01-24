using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure;

public class UserGroup
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
}