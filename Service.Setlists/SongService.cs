using Amazon.Lambda.Core;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using Service.Setlists.Exceptions;

namespace Service.Setlists;

/// <summary>
/// Service class to manage Songs and variants
/// </summary>
public class SongService(ISongRepository songRepository, ISongVariantRepository songVariantRepository, ILambdaLogger logger)
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
}