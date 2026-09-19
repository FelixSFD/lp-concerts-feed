using System.Runtime.CompilerServices;
using Amazon.Runtime.Internal;
using Common.WikiMedia;
using Common.WikiMedia.Repositories;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure;
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
    IConcertRepository concertRepository,
    ILogger<LinkinpediaImportConcertService> logger)
{
    /// <summary>
    /// Returns a list of concerts on Linkinpedia and information on whether they are already imported
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<ConcertImportStatusBo> GetImportStatusList(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting list of concerts on Linkinpedia...");
        string[] tables = ["Shows"];
        string[] fields = ["Artist", "ShowPage", "Date", "ShowType", "Country", "State", "Province", "UKCountry", "City", "Venue", "Tour", "TourLeg"];
        CargoQueryWhereClause[] where = [
            new()
            {
                FieldName = "Artist",
                Value = "Linkin Park",
                Comparison = CargoQueryWhereClause.Operation.IsEqual,
            }
        ];
        string[] orderBy = ["Date"];
        
        logger.LogDebug("Preloading concert list...");
        var concertByWikiPage = await concertRepository
            .FindAllWithReferencesAsync(cancellationToken)
            .Where(c => c.LinkinpediaUrl != null)
            .ToDictionaryAsync(c => c.LinkinpediaUrl?.ToString().Split('/').LastOrDefault() ?? "none", c => c, null, cancellationToken);
        logger.LogDebug("Preloaded concerts for {concertCount} wiki pages.", concertByWikiPage.Count);
        
        var results = wikiMediaRepository
            .RunCargoQueryAsync<LinkinpediaConcertListEntryBo>(tables, fields, where, orderBy, pageSize: 100, cancellationToken)
            .Select(entry => entry.Value)
            .Select(concert =>
            {
                var concertImported = concertByWikiPage.TryGetValue(concert.WikiPageId, out var existingConcert);
                
                var resultItem = new ConcertImportStatusBo
                {
                    WikiPageId = concert.WikiPageId,
                    ConcertTitle = GenerateConcertTitle(concert.DateString, concert.Venue, concert.City, concert.Country),
                    ImportStatus = concertImported
                        ? ConcertImportStatusBo.Status.Imported
                        : ConcertImportStatusBo.Status.NotImported,
                    Concert = existingConcert?.ToBoWithDetails(),
                };
                return resultItem;
            });

        await foreach (var result in results)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("Cancellation requested, stopping iteration.");
                yield break;
            }
            
            yield return result;
        }
    }

    private static string? GenerateConcertTitle(string dateString, string? venueName, string? cityName,
        string? countryName)
    {
        string?[] parts = [dateString, venueName, cityName, countryName];
        var partsNotNull = parts.Where(part => !string.IsNullOrWhiteSpace(part)).Cast<string>().ToArray();
        return partsNotNull.Length == 0 ? null : string.Join(", ", partsNotNull);
    }
    
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
        var cityNativeName = tourdate.CityLocal;
        var cityNameRaw = tourdate.City;
        string? cityName = cityNameRaw;
        string? stateName = null;
        var concertStatus = ConcertDto.ConcertStatusValue.Planned;

        var eventName = tourdate.Event;

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
        var tourLegName = tourdate.TourLeg;
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
            .Where(t =>
                string.Equals(t.Name, tourLegName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Id, tourLegName, StringComparison.OrdinalIgnoreCase)
            )
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
            else if (tourdate.ShowType.Contains("concert", StringComparison.OrdinalIgnoreCase))
            {
                var lpShowType = concertTypes.FirstOrDefault(ct => ct.Name.Contains("Linkin Park Show", StringComparison.OrdinalIgnoreCase));
                if (lpShowType != null)
                    concertType = lpShowType.ToBo();
            }
            else
            {
                var showTypeMatch = concertTypes.FirstOrDefault(ct => string.Equals(ct.Name, tourdate.ShowType, StringComparison.OrdinalIgnoreCase));
                if (showTypeMatch != null)
                    concertType = showTypeMatch.ToBo();
            }
        }

        if (tourdate.ShowType == "cancelled")
        {
            concertStatus = ConcertDto.ConcertStatusValue.Cancelled;
        }

        if (concertType == null)
        {
            var lpShowType = concertTypes.FirstOrDefault(ct => string.Equals(ct.Name, "Other", StringComparison.OrdinalIgnoreCase));
            if (lpShowType != null)
                concertType = lpShowType.ToBo();
        }

        return new ImportConcertPreviewBo
        {
            ConcertStatus = concertStatus,
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
            CityNativeName = cityNativeName,
            VenueName = venueName,
            TourName = tourName,
            TourLegName = tourLegName,
            ProposedCustomTitle = eventName,
        };
    }
}