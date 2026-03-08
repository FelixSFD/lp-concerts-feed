namespace Lambda.SetlistsWrite.Services.Exceptions;

/// <summary>
/// Exception that is thrown when a setlist was not found
/// </summary>
/// <param name="setlistId">ID the setlist</param>
public class SetlistNotFoundException(uint setlistId) : SetlistServiceException
{
    /// <summary>
    /// ID of the setlist that was not found
    /// </summary>
    public uint SetlistId { get; set; } = setlistId;
}