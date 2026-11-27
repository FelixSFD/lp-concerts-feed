using Service.Tours.DataStructure.Stats;

namespace Service.Tours;

/// <summary>
/// Service to calculate personalized statistics for users
/// </summary>
[Obsolete("Use StatisticsService instead")]
public class UserStatsService(StatisticsService statisticsService)
{
    /// <summary>
    /// Returns statistics about the concerts a user has attended
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    public async Task<UserConcertStatsBo> GetAttendedConcerts(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var stats = await statisticsService.GetConcertStatsAsync(new StatsScope(userId: userId), cancellationToken);

        return new UserConcertStatsBo
        {
            Concerts = stats.Concerts,
            NumberOfCities = stats.NumberOfCities,
            NumberOfCountries = stats.NumberOfCountries,
            NumberOfVenues = stats.NumberOfVenues,
            NumberOfConcerts = stats.NumberOfConcerts,
        };
    }
    
    /// <summary>
    /// Calculates how many times a user has seen each song live, based on the concerts they have attended.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>List of song names and play count</returns>
    public async Task<UserSongStatBo[]> GetSongsUserHasSeenLiveAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var songs = await statisticsService.GetSongStatsAsync(new StatsScope(userId: userId), cancellationToken);
        var stats = songs.Select(song => new UserSongStatBo
        {
            SongId = song.SongId,
            SongName = song.SongName,
            AlbumName = song.AlbumName,
            AppleMusicId = song.AppleMusicId,
            PlayCount = song.PlayCount,
        }).ToArray();
        return stats;
    }

    /// <summary>Calculates concert and song statistics using one attended-concert selection.</summary>
    public async Task<(ConcertStatsBo Concerts, SongStatBo[] Songs)> GetConcertAndSongStatsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await statisticsService.GetConcertAndSongStatsAsync(new StatsScope(userId: userId), cancellationToken);
    }
}