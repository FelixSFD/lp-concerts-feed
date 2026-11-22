namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Updated fields of an act in a setlist.
/// </summary>
public class UpdateSetlistActRequestBo
{
    /// <summary>
    /// Title of the act, or null to clear it.
    /// </summary>
    public string? Title { get; set; }
}
