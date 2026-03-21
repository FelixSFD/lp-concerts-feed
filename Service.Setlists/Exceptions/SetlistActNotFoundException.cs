namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception that is thrown when an act was not found
/// </summary>
/// <param name="setlistId">ID the setlist</param>
/// <param name="actNumber">number of the act</param>
public class SetlistActNotFoundException(uint setlistId, uint actNumber) : SetlistServiceException($"Act number {actNumber} not found in setlist with ID '{setlistId}'!")
{
    /// <summary>
    /// ID of the setlist that was not found
    /// </summary>
    public uint SetlistId { get; set; } = setlistId;
    
    /// <summary>
    /// Number of the act within <see cref="SetlistId"/> that was not found
    /// </summary>
    public uint ActNumber { get; set; } = actNumber;
}