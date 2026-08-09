namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Type of concert
/// </summary>
public class ConcertTypeBo
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public uint Id { get; set; }
    
    /// <summary>
    /// Displayed name
    /// </summary>
    public required string Name { get; set; }
}