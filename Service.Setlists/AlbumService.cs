using Amazon.Lambda.Core;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using Service.Setlists.Exceptions;

namespace Service.Setlists;

/// <summary>
/// Service class to manage Albums
/// </summary>
public class AlbumService(IAlbumRepository albumRepository, ILambdaLogger logger)
{
    /// <summary>
    /// Creates a new album
    /// </summary>
    /// <param name="request">Request to create the album</param>
    public async Task<AlbumDto> CreateAlbumAsync(CreateAlbumRequestDto request)
    {
        logger.LogDebug("Creating album with title '{title}'...", request.Title);

        var album = new AlbumDo
        {
            Title = request.Title,
            LinkinpediaUrl = request.LinkinpediaUrl
        };
        
        albumRepository.Add(album);
        await albumRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created album with ID '{id}' and title '{title}'.", album.Id, album.Title);
        return DtoMapper.ToDto(album);
    }
    
    /// <summary>
    /// Returns an album by its ID
    /// </summary>
    /// <param name="albumId">ID of the album</param>
    /// <returns></returns>
    /// <exception cref="AlbumNotFoundException">if the album was not found</exception>
    public async Task<AlbumDto> GetAlbumById(uint albumId)
    {
        logger.LogDebug("Getting album with id: {albumId}", albumId);
        var album = await albumRepository.GetByPrimaryKeyAsync(albumId) ?? throw new AlbumNotFoundException(albumId);
        return DtoMapper.ToDto(album);
    }
    
    /// <summary>
    /// Returns all albums ordered by title
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Enumerable of all albums</returns>
    public IAsyncEnumerable<AlbumDto> GetAllAlbumsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all albums...");
        return albumRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto)
            .OrderBy(s => s.Title);
    }
    
    /// <summary>
    /// Deletes an album
    /// </summary>
    /// <param name="albumId">ID of the album</param>
    public async Task DeleteAlbumWithIdAsync(uint albumId)
    {
        logger.LogDebug("Delete album with id: {albumId}", albumId);
        var album = await albumRepository.GetByPrimaryKeyAsync(albumId);
        if (album != null)
        {
            albumRepository.Delete(album);
            await albumRepository.SaveChangesAsync();
            
            logger.LogInformation("Album with ID '{id}' was deleted successfully.", albumId);
            return;
        }
        
        logger.LogInformation("Album with ID '{id}' does not exist.", albumId);
    }
}