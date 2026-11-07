using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours;

/// <summary>
/// Service to calculate personalized statistics for users
/// </summary>
public class UserStatsService(IConcertRepository concertRepository, ILogger<UserStatsService> logger)
{
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
            Concerts = [.. concerts.Select(DoMapper.ToBo)],
            NumberOfCities = numberOfCities,
            NumberOfCountries = numberOfCountries,
            NumberOfVenues = numberOfVenues,
            NumberOfConcerts = numberOfConcerts,
        };
    }
}