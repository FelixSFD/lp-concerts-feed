namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception that is thrown when a song mashup was not found
/// </summary>
/// <param name="songMashupId">ID the song mashup</param>
public class SongMashupNotFoundException(uint songMashupId) : SetlistServiceException($"The mashup with ID '{songMashupId}' was not found!")
{
    /// <summary>
    /// ID of the song mashup that was not found
    /// </summary>
    public uint SongMashupId { get; set; } = songMashupId;
}