using Amazon.Lambda.Core;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using Service.Setlists.Exceptions;

namespace Service.Setlists;

/// <summary>
/// Service class to manage Songs, variants and mashups
/// </summary>
public class SongService(ISongRepository songRepository, ISongVariantRepository songVariantRepository, ISongMashupRepository songMashupRepository, ILambdaLogger logger)
{
    /// <summary>
    /// Creates a new song
    /// </summary>
    /// <param name="request">Request to create the song</param>
    public async Task<SongDto> CreateSongAsync(CreateSongRequestDto request)
    {
        logger.LogDebug("Creating song with title '{title}'...", request.Title);

        var song = new SongDo
        {
            Title = request.Title,
            AlbumId = request.AlbumId,
            Isrc = request.Isrc,
            LinkinpediaUrl = request.LinkinpediaUrl
        };
        
        songRepository.Add(song);
        await songRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created song '{id}'.", song.Id);
        return DtoMapper.ToDto(song);
    }
    
    /// <summary>
    /// Returns all songs ordered by title
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Enumerable of all songs</returns>
    public IAsyncEnumerable<SongDto> GetAllSongsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all songs...");
        return songRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto)
            .OrderBy(s => s.Title);
    }
    
    /// <summary>
    /// Returns a song by its ID
    /// </summary>
    /// <param name="songId">ID of the song</param>
    /// <returns></returns>
    /// <exception cref="SongNotFoundException">if the song was not found</exception>
    public async Task<SongDto> GetSongById(uint songId)
    {
        logger.LogDebug("Getting song with id: {songId}", songId);
        var song = await songRepository.GetByPrimaryKeyAsync(songId) ?? throw new SongNotFoundException(songId);
        return DtoMapper.ToDto(song);
    }
    
    /// <summary>
    /// Updates the information of a song
    /// </summary>
    /// <param name="songId">ID of the song to update</param>
    /// <param name="request">new information for the song</param>
    /// <returns>the saved song</returns>
    /// <exception cref="SongNotFoundException">if the song does not exist</exception>
    public async Task<SongDto> UpdateSongAsync(uint songId, UpdateSongRequestDto request)
    {
        logger.LogDebug("Load song with id: {songId}", songId);
        var song = await songRepository.GetByPrimaryKeyAsync(songId) ?? throw new SongNotFoundException(songId);
        song.Title = request.Title;
        song.AlbumId = request.AlbumId;
        song.Isrc = request.Isrc;
        song.LinkinpediaUrl = request.LinkinpediaUrl;
        logger.LogDebug("Save song...");
        songRepository.Update(song);

        await songRepository.SaveChangesAsync();
        logger.LogDebug("Successfully saved song.");
        
        return DtoMapper.ToDto(song);
    }
    
    /// <summary>
    /// Deletes a song
    /// </summary>
    /// <param name="songId">ID of the song</param>
    public async Task DeleteSongWithIdAsync(uint songId)
    {
        logger.LogDebug("Delete song with id: {songId}", songId);
        var song = await songRepository.GetByPrimaryKeyAsync(songId);
        if (song != null)
        {
            songRepository.Delete(song);
            await songRepository.SaveChangesAsync();
            
            logger.LogInformation("Song '{id}' was deleted successfully.", songId);
            return;
        }
        
        logger.LogInformation("Song '{id}' does not exist.", songId);
    }
    
    /// <summary>
    /// Returns a song variant by its ID
    /// </summary>
    /// <param name="variantId">ID of the song</param>
    /// <returns></returns>
    /// <exception cref="SongVariantNotFoundException">if the song variant was not found</exception>
    public async Task<SongVariantDto> GetSongVariantById(uint variantId)
    {
        logger.LogDebug("Getting song variant with id: {variantId}", variantId);
        var song = await songVariantRepository.GetByPrimaryKeyAsync(variantId) ?? throw new SongVariantNotFoundException(variantId);
        return DtoMapper.ToDto(song);
    }
    
    /// <summary>
    /// Returns all variants of a given song
    /// </summary>
    /// <param name="songId">ID of the song</param>
    /// <returns></returns>
    /// <exception cref="SongNotFoundException">if the song was not found</exception>
    public async Task<List<SongVariantDto>> GetVariantsOfSong(uint songId)
    {
        logger.LogDebug("Getting song variant for song with id: {songId}", songId);
        var variants = await songVariantRepository.GetVariantsOfSongAsync(songId) ?? throw new SongVariantNotFoundException(songId);
        return variants
            .Select(DtoMapper.ToDto)
            .ToList();
    }

    /// <summary>
    /// Creates a new mashup of two or more songs
    /// </summary>
    /// <param name="request">Request to create the mashup</param>
    /// <exception cref="InvalidMashupException">if the requested mashup was not allowed</exception>
    public async Task<SongMashupDto> CreateMashupOfSongsAsync(CreateSongMashupRequestDto request)
    {
        var ids = request.SongIds;
        logger.LogDebug("Creating mashup of {count} songs...", ids.Length);
        if (ids.Length < 2)
        {
            logger.LogError("Could not create mashup. A mashup must consist of at least 2 songs. Requested {count} songs.", ids.Length);
            throw new InvalidMashupException("A mashup must consist of at least 2 songs.");
        }

        var songs = await songRepository.GetSongsByIds(ids).ToArrayAsync();
        logger.LogDebug("Loaded {count} songs.", songs.Length);

        var mashup = new SongMashupDo
        {
            Title = request.Title,
            LinkinpediaUrl = request.LinkinpediaUrl,
            Songs = songs
        };
        
        songMashupRepository.Add(mashup);
        await songMashupRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created mashup '{id}' with {count} songs.", mashup.Id, songs.Length);
        return DtoMapper.ToDto(mashup);
    }
    
    /// <summary>
    /// Returns all mashups
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Enumerable of all mashups</returns>
    public IAsyncEnumerable<SongMashupDto> GetAllSongMashupsAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all mashups...");
        return songMashupRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto);
    }
    
    /// <summary>
    /// Returns a mashup
    /// </summary>
    /// <param name="mashupId">ID of the mashup</param>
    /// <returns>Enumerable of all mashups</returns>
    public async Task<SongMashupDto?> GetMashupByIdAsync(uint mashupId)
    {
        logger.LogDebug("Find mashup with id: {mashupId}", mashupId);
        var mashup = await songMashupRepository.GetByPrimaryKeyAsync(mashupId);
        if (mashup != null)
            return DtoMapper.ToDto(mashup);
        
        logger.LogWarning("Mashup '{id}' does not exist.", mashupId);
        return null;
    }
    
    /// <summary>
    /// Deletes a mashup
    /// </summary>
    /// <param name="mashupId">ID of the mashup</param>
    public async Task DeleteMashupWithIdAsync(uint mashupId)
    {
        logger.LogDebug("Delete mashup with id: {mashupId}", mashupId);
        var mashup = await songMashupRepository.GetByPrimaryKeyAsync(mashupId);
        if (mashup != null)
        {
            songMashupRepository.Delete(mashup);
            await songMashupRepository.SaveChangesAsync();
            
            logger.LogInformation("Mashup '{id}' was deleted successfully.", mashupId);
            return;
        }
        
        logger.LogInformation("Mashup '{id}' does not exist.", mashupId);
    }

    /// <summary>
    /// Updates the information of a mashup
    /// </summary>
    /// <param name="mashupId">ID of the mashup to update</param>
    /// <param name="request">new information for the mashup</param>
    /// <returns>the saved mashup</returns>
    /// <exception cref="SongMashupNotFoundException">if the mashup does not exist</exception>
    public async Task<SongMashupDto> UpdateMashupAsync(uint mashupId, UpdateSongMashupRequestDto request)
    {
        logger.LogDebug("Load mashup with id: {mashupId}", mashupId);
        var mashup = await songMashupRepository.GetByPrimaryKeyAsync(mashupId) ?? throw new SongMashupNotFoundException(mashupId);
        logger.LogDebug("Load {count} songs in the mashup...", request.SongIds.Length);
        var songs = await songRepository
            .GetSongsByIds(request.SongIds)
            .ToArrayAsync();
        logger.LogDebug("Found {count} songs in the mashup.", songs.Length);
        mashup.Title = request.Title;
        mashup.LinkinpediaUrl = request.LinkinpediaUrl;
        mashup.Songs = songs;
        logger.LogDebug("Save mashup...");
        songMashupRepository.Update(mashup);

        await songMashupRepository.SaveChangesAsync();
        logger.LogDebug("Successfully saved mashup.");
        
        return DtoMapper.ToDto(mashup);
    }
}