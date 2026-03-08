namespace Lambda.SetlistsWrite.Services.Exceptions;

/// <summary>
/// Exception that is thrown when a song was not found
/// </summary>
/// <param name="songId">ID the song</param>
public class SongNotFoundException(uint songId) : SetlistServiceException($"The Song with ID {songId} was not found!")
{
    /// <summary>
    /// ID of the song that was not found
    /// </summary>
    public uint SongId { get; set; } = songId;
}