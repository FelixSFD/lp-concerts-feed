using Amazon.Lambda.Core;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using Service.Setlists.Exceptions;

namespace Service.Setlists;

/// <summary>
/// Service class to manage Songs and variants
/// </summary>
public class SongService(ISongRepository songRepository, ISongVariantRepository songVariantRepository, ISongMashupRepository songMashupRepository, ILambdaLogger logger)
{
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
}