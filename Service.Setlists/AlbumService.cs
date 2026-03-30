using Amazon.Lambda.Core;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;

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
}