namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception that is thrown when a song variant was not found
/// </summary>
/// <param name="songVariantId">ID the song variant</param>
public class SongVariantNotFoundException(uint songVariantId) : SetlistServiceException
{
    /// <summary>
    /// ID of the song variant that was not found
    /// </summary>
    public uint SongVariantId { get; set; } = songVariantId;
}