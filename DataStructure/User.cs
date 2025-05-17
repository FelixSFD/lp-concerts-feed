using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}