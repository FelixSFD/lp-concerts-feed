namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception that is thrown when a setlist was not found
/// </summary>
/// <param name="setlistId">ID the setlist</param>
/// <param name="entryId">ID of the entry</param>
public class SetlistEntryNotFoundException(uint setlistId, string entryId) : SetlistServiceException($"The setlist entry with ID '{entryId}' was not found!")
{
    /// <summary>
    /// ID of the setlist that was not found
    /// </summary>
    public uint SetlistId { get; set; } = setlistId;
    
    /// <summary>
    /// ID of the setlist entry that was not found
    /// </summary>
    public string EntryId { get; set; } = entryId;
}