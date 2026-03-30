namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception that is thrown when an album was not found
/// </summary>
/// <param name="albumId">ID the song</param>
public class AlbumNotFoundException(uint albumId) : SetlistServiceException($"The Album with ID {albumId} was not found!")
{
    /// <summary>
    /// ID of the song that was not found
    /// </summary>
    public uint AlbumId { get; set; } = albumId;
}