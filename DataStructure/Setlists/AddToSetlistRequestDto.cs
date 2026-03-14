using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Base class for requests to add entries to a setlist
/// </summary>
public abstract class AddToSetlistRequestDto
{
    /// <summary>
    /// If set, the added song will be added to this Act.
    /// Title isn't required if the Act already exists.
    /// </summary>
    [JsonPropertyName("actParameters")]
    public ActParametersDto? Act { get; set; }

    
    /// <summary>
    /// Common parameters for that entry, no matter if it was a song or mashup
    /// </summary>
    [JsonPropertyName("entryParameters")]
    public required SetlistEntryParametersDto EntryParameters { get; set; }
}