using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Setlists;
using Microsoft.Extensions.Logging;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours;

/// <summary>
/// Service to calculate personalized statistics for users
/// </summary>
public class UserStatsService(IConcertRepository concertRepository, ISongRepository songRepository, ILogger<UserStatsService> logger)
{
    /// <summary>
    /// Returns statistics about the concerts a user has attended
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    public async Task<UserConcertStatsBo> GetAttendedConcerts(string userId, CancellationToken cancellationToken = default)
    {
        Log.StartGeneratingAttendedConcertsStats(logger, userId);

        var concerts = await concertRepository
            .GetAttendedConcertsForAsync(userId, cancellationToken)
            .ToArrayAsync(cancellationToken);
        
        Log.LoadedConcertsUserAttended(logger, userId);

        var numberOfConcerts = concerts.Length;
        var byVenue = concerts
            .GroupBy(c => c.Venue, c => c)
            .ToDictionary(g => g.Key, g => g.ToArray());
        var byCity = byVenue
            .Select(g => g.Key)
            .GroupBy(v => v.City, v => v)
            .ToDictionary(g => g.Key, g => g.ToArray());
        var byCountry = byCity
            .Select(c => c.Key)
            .GroupBy(c => c.CountryCode, c => c)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var numberOfVenues = byVenue
            .Select(v => v.Key.Id)
            .Distinct()
            .Count();
        var numberOfCities = byVenue
            .Select(c => c.Key.Id)
            .Distinct()
            .Count();
        var numberOfCountries = byCountry
            .Select(c => c.Key)
            .Distinct()
            .Count();
        
        Log.FinishedGeneratingAttendedConcertsStats(logger, userId, numberOfConcerts, numberOfVenues, numberOfCities, numberOfCountries);

        return new UserConcertStatsBo
        {
            Concerts = [.. concerts.Select(DoMapper.ToBoWithDetails)],
            NumberOfCities = numberOfCities,
            NumberOfCountries = numberOfCountries,
            NumberOfVenues = numberOfVenues,
            NumberOfConcerts = numberOfConcerts,
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
        Log.StartGeneratingAttendedSongStats(logger, userId);
        
        var concertIds = await concertRepository
            .GetAttendedConcertsForAsync(userId, cancellationToken)
            .Select(c => c.Id)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        
        Log.LoadedConcertsUserAttended(logger, userId);
        
        var songsPlayed = await songRepository.GetSongsPlayedAtConcertsAsync(concertIds, cancellationToken);
        var stats = songsPlayed.Select(GetSongStatFromDictEntry).ToArray();
        Log.FinishedGeneratingAttendedSongStats(logger, userId, stats.Length);
        return stats;
    }
    
    private static UserSongStatBo GetSongStatFromDictEntry(KeyValuePair<SongDo, uint> kv)
    {
        var song = kv.Key;
        var count = kv.Value;
        return new UserSongStatBo
        {
            SongName = song.Title,
            PlayCount = count,
        };
    }
}