using Common.WikiMedia.Repositories;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;
using Service.Tours.DataStructure;
using Service.Tours.Importer;

namespace Service.Tours;

/// <summary>
/// Service to import concert information from Linkinpedia
/// </summary>
/// <param name="wikiMediaRepository"></param>
/// <param name="wikitextParser"></param>
/// <param name="countryRepository"></param>
/// <param name="stateRepository"></param>
/// <param name="cityRepository"></param>
/// <param name="venueRepository"></param>
/// <param name="tourRepository"></param>
/// <param name="concertTypeRepository"></param>
/// <param name="logger"></param>
public class LinkinpediaImportConcertService(
    IWikiMediaRepository wikiMediaRepository,
    TourdataWikitextParser wikitextParser,
    ICountryRepository countryRepository,
    IStateRepository stateRepository,
    ICityRepository cityRepository,
    IVenueRepository venueRepository,
    ITourRepository tourRepository,
    IConcertTypeRepository concertTypeRepository,
    ILogger<LinkinpediaImportConcertService> logger)
{
    public async Task<ImportConcertPreviewBo> GetConcertImportPlan(string wikiPageId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Generating concert import plan for concert: {page}", wikiPageId);
        
        var wikiPage = await wikiMediaRepository.GetWikiPageAsync(wikiPageId);
        if (wikiPage == null || string.IsNullOrWhiteSpace(wikiPage.Source))
        {
            logger.LogWarning("Wiki page '{pageTitle}' was not found or has empty source.", wikiPageId);
            return new ImportConcertPreviewBo
            {
                FoundCountries = [],
                FoundStates = [],
                FoundCities = [],
                FoundVenues = [],
                FoundTours = [],
                FoundTourLegs = []
            };
        }

        var parser = wikitextParser;
        var tourdate = parser.GetTourdateInformation(wikiPage.Source);
        if (tourdate == null)
        {
            logger.LogWarning("Could not parse Tourdate information from wiki page '{pageTitle}'.", wikiPageId);
            return new ImportConcertPreviewBo
            {
                FoundCountries = [],
                FoundStates = [],
                FoundCities = [],
                FoundVenues = [],
                FoundTours = [],
                FoundTourLegs = []
            };
        }

        var countryName = tourdate.Country;
        var cityNameRaw = tourdate.City;
        string? cityName = cityNameRaw;
        string? stateName = null;

        if (!string.IsNullOrWhiteSpace(cityNameRaw))
        {
            var commaIndex = cityNameRaw.IndexOf(',');
            if (commaIndex >= 0)
            {
                cityName = cityNameRaw[..commaIndex].Trim();
                stateName = cityNameRaw[(commaIndex + 1)..].Trim();
            }
        }

        var venueName = tourdate.Venue;
        var tourName = tourdate.Tour;
        string? tourLegName = null;
        DateTimeOffset postedStartTime = default;

        if (tourdate.Date.HasValue)
        {
            postedStartTime = new DateTimeOffset(tourdate.Date.Value.ToDateTime(new TimeOnly(20, 0)), TimeSpan.Zero);
        }

        // Search database for matching entities
        var countries = await countryRepository.QueryAsync(cancellationToken).ToListAsync(cancellationToken);
        var matchingCountries = string.IsNullOrWhiteSpace(countryName)
            ? []
            : countries.Where(c =>
                string.Equals(c.Name, countryName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.NativeName, countryName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.IsoCode, countryName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        var foundCountries = matchingCountries.Select(DoMapper.ToBo).ToArray();

        var states = await stateRepository.QueryAsync(cancellationToken).ToListAsync(cancellationToken);
        var matchingStates = string.IsNullOrWhiteSpace(stateName)
            ? []
            : states.Where(s =>
                string.Equals(s.Code, stateName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.Name, stateName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.NativeName, stateName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        var foundStates = matchingStates.Select(DoMapper.ToBo).ToArray();

        var cities = await cityRepository.QueryAsync(cancellationToken).ToListAsync(cancellationToken);
        var matchingCities = string.IsNullOrWhiteSpace(cityName)
            ? []
            : cities.Where(c =>
                string.Equals(c.Name, cityName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.NativeName, cityName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        var foundCities = matchingCities.Select(DoMapper.ToDtoWithCountry).ToArray();

        var venues = await venueRepository.QueryAsync(cancellationToken).ToListAsync(cancellationToken);
        var venueLookupName = tourdate.Venue;
        var venueIdLookupName = tourdate.VenueId;
        var matchingVenues = venues.Where(v =>
            (!string.IsNullOrWhiteSpace(venueLookupName) && (
                string.Equals(v.CurrentName, venueLookupName, StringComparison.OrdinalIgnoreCase) ||
                v.PreviousNames.Any(pn => string.Equals(pn.Name, venueLookupName, StringComparison.OrdinalIgnoreCase))
            )) ||
            (!string.IsNullOrWhiteSpace(venueIdLookupName) && (
                string.Equals(v.CurrentName, venueIdLookupName, StringComparison.OrdinalIgnoreCase) ||
                v.PreviousNames.Any(pn => string.Equals(pn.Name, venueIdLookupName, StringComparison.OrdinalIgnoreCase))
            ))
        ).ToList();
        var foundVenues = matchingVenues.Select(DoMapper.ToBo).ToArray();

        var tours = await tourRepository.QueryAsync(cancellationToken).ToListAsync(cancellationToken);
        var matchingTours = string.IsNullOrWhiteSpace(tourName)
            ? []
            : tours.Where(t =>
                string.Equals(t.Name, tourName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Id, tourName, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        var foundTours = matchingTours.Select(DoMapper.ToBo).ToArray();
        var foundTourLegs = matchingTours
            .SelectMany(t => t.Legs)
            .Select(DoMapper.ToBo)
            .ToArray();

        var concertTypes = await concertTypeRepository.QueryAsync(cancellationToken).ToListAsync(cancellationToken);
        ConcertTypeBo? concertType = null;
        logger.LogDebug("Found {concertTypes} concert types. Source uses {sourceConcertType}", concertTypes.Count, tourdate.ShowType);
        if (!string.IsNullOrWhiteSpace(tourdate.ShowType))
        {
            if (tourdate.ShowType.Contains("festival", StringComparison.OrdinalIgnoreCase))
            {
                var festivalType = concertTypes.FirstOrDefault(ct => ct.Name.Contains("Festival", StringComparison.OrdinalIgnoreCase));
                if (festivalType != null)
                    concertType = festivalType.ToBo();
            }
            else
            {
                var showTypeMatch = concertTypes.FirstOrDefault(ct => string.Equals(ct.Name, tourdate.ShowType, StringComparison.OrdinalIgnoreCase));
                if (showTypeMatch != null)
                    concertType = showTypeMatch.ToBo();
            }
        }

        if (concertType == null)
        {
            var lpShowType = concertTypes.FirstOrDefault(ct => string.Equals(ct.Name, "Linkin Park Show", StringComparison.OrdinalIgnoreCase));
            if (lpShowType != null)
                concertType = lpShowType.ToBo();
        }

        return new ImportConcertPreviewBo
        {
            ConcertType = concertType,
            PostedStartTime = postedStartTime,
            FoundCountries = foundCountries,
            FoundStates = foundStates,
            FoundCities = foundCities,
            FoundVenues = foundVenues,
            FoundTours = foundTours,
            FoundTourLegs = foundTourLegs,
            CountryName = countryName,
            StateName = stateName,
            CityName = cityName,
            VenueName = venueName,
            TourName = tourName,
            TourLegName = tourLegName,
            ProposedCustomTitle = null
        };
    }
}