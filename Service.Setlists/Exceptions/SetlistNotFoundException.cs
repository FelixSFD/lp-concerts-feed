namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception that is thrown when a setlist was not found
/// </summary>
/// <param name="setlistId">ID the setlist</param>
public class SetlistNotFoundException(uint setlistId) : SetlistServiceException($"The setlist with ID {setlistId} was not found!")
{
    /// <summary>
    /// ID of the setlist that was not found
    /// </summary>
    public uint SetlistId { get; set; } = setlistId;
}