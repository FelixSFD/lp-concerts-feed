using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Parameters;

/// <summary>
/// Set of properties used whenever data of an act is passed to the server.
/// Title isn't required if the Act already exists
/// </summary>
public class ActParametersDto
{
    /// <summary>
    /// ID of the setlist. Does not need to be passed if this object is included in a request that already contains the ID of the setlist.
    /// </summary>
    [JsonPropertyName("setlistId")]
    public uint? SetlistId { get; set; }
    
    
    /// <summary>
    /// Unique ID of the setlist
    /// </summary>
    [JsonPropertyName("actNumber")]
    public uint ActNumber { get; set; }


    /// <summary>
    /// Title of this act
    /// </summary>
    [MaxLength(31)]
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}