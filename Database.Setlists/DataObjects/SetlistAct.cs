namespace Database.Setlists.DataObjects;

public class SetlistAct
{
    /// <summary>
    /// ID of the setlist
    /// </summary>
    public uint SetlistId { get; set; }
    
    
    /// <summary>
    /// Unique ID of the setlist
    /// </summary>
    public uint ActNumber { get; set; }


    /// <summary>
    /// Title of this act
    /// </summary>
    public string? Title { get; set; }
}