using Common.WikiMedia;
using Common.WikiMedia.DTOs;
using Common.WikiMedia.Repositories;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.DataStructure;
using Service.Tours.Importer;

namespace Service.Tours.Tests;

public class LinkinpediaImportConcertServiceTest
{
    private readonly IWikiMediaRepository _wikiMediaRepository;
    private readonly TourdataWikitextParser _wikitextParser;
    private readonly ICountryRepository _countryRepository;
    private readonly IStateRepository _stateRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IConcertTypeRepository _concertTypeRepository;
    private readonly IConcertRepository _concertRepository;
    private readonly ILogger<LinkinpediaImportConcertService> _logger;
    private readonly LinkinpediaImportConcertService _sut;

    public LinkinpediaImportConcertServiceTest()
    {
        _wikiMediaRepository = Substitute.For<IWikiMediaRepository>();
        _wikitextParser = new TourdataWikitextParser();
        _countryRepository = Substitute.For<ICountryRepository>();
        _stateRepository = Substitute.For<IStateRepository>();
        _cityRepository = Substitute.For<ICityRepository>();
        _venueRepository = Substitute.For<IVenueRepository>();
        _tourRepository = Substitute.For<ITourRepository>();
        _concertTypeRepository = Substitute.For<IConcertTypeRepository>();
        _concertRepository = Substitute.For<IConcertRepository>();
        _logger = Substitute.For<ILogger<LinkinpediaImportConcertService>>();

        _sut = new LinkinpediaImportConcertService(
            _wikiMediaRepository,
            _wikitextParser,
            _countryRepository,
            _stateRepository,
            _cityRepository,
            _venueRepository,
            _tourRepository,
            _concertTypeRepository,
            _concertRepository,
            _logger
        );
    }

    private const string SampleUsConcertWikitext = """
        {{Tourdate
        | ShowType = concert
        | Last show = 2004.08.30
        | Artist = Linkin Park
        | Next show = 2004.09.03
        | Year = 2004
        | Month = September
        | Day = 01
        | Country = United States
        | City = Phoenix, AZ
        | Venue = Cricket Pavilion
        | VenueID = Cricket Wireless Pavilion
        | Venue Type = Amphitheatre
        | Venue Website = http://www.cricket-pavilion.com/
        | Tour = Projekt Revolution 2004
        | Stage = Main Stage
        | Other artists = Korn, Snoop Dogg, The Used, Less Than Jake
        }}

        == Setlist ==
        {{Setlist
        | Song1 = Don't Stay
        }}
        """;

    private const string SampleBerlinConcertWikitext = """
        {{Tourdate
        | Last show = 2025.06.16
        | Next show = 2025.06.20
        | Artist = Linkin Park
        | ShowType = concert
        | Year = 2025
        | Month = June
        | Day = 18
        | Country = Germany
        | City = Berlin
        | Venue = Olympiastadion
        | Venue Type = Stadium
        | Tour = From Zero World Tour
        | Support = Architects
        | Support2 = grandson
        | Setlist = B6
        }}
        """;

    [Fact]
    public async Task GetConcertImportPlan_WhenUsConcert_ExtractsAndMatchesCorrectly()
    {
        var pageId = "Live:20040901";

        var mockWikiPage = new WikiPageDto
        {
            Id = 100,
            Title = pageId,
            Source = SampleUsConcertWikitext
        };

        _wikiMediaRepository.GetWikiPageAsync(pageId).Returns(mockWikiPage);

        var countryUs = new CountryDo
        {
            IsoCode = "USA",
            Name = "United States",
            NativeName = "United States"
        };
        _countryRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { countryUs }.ToAsyncEnumerable());

        var stateAz = new StateDo
        {
            CountryCode = "USA",
            Code = "AZ",
            Name = "Arizona",
            NativeName = "Arizona",
            Country = countryUs
        };
        _stateRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { stateAz }.ToAsyncEnumerable());

        var cityPhoenix = new CityDo
        {
            Id = 10,
            CountryCode = "USA",
            StateCode = "AZ",
            Name = "Phoenix",
            NativeName = "Phoenix",
            Country = countryUs,
            State = stateAz
        };
        _cityRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { cityPhoenix }.ToAsyncEnumerable());

        var venueCricket = new VenueDo
        {
            Id = 50,
            CountryCode = "USA",
            StateCode = "AZ",
            CityId = 10,
            CurrentName = "Cricket Wireless Pavilion",
            TimeZone = "America/Phoenix",
            Country = countryUs,
            State = stateAz,
            City = cityPhoenix,
            PreviousNames = new List<PreviousVenueNameDo>
            {
                new() { Id = 1, Name = "Cricket Pavilion", From = new DateOnly(2000, 1, 1), To = new DateOnly(2006, 1, 1) }
            }
        };
        _venueRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { venueCricket }.ToAsyncEnumerable());

        var tourPR04 = new TourDo
        {
            Id = "pr-2004",
            Name = "Projekt Revolution 2004",
            Legs = new List<TourLegDo>
            {
                new() { Id = "pr-2004-us", Name = "North American Tour", TourId = "pr-2004" }
            }
        };
        _tourRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { tourPR04 }.ToAsyncEnumerable());

        var concertTypeLp = new ConcertTypeDo
        {
            Id = 1,
            Name = "Linkin Park Show"
        };
        var concertTypeFestival = new ConcertTypeDo
        {
            Id = 2,
            Name = "Festival"
        };
        _concertTypeRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { concertTypeLp, concertTypeFestival }.ToAsyncEnumerable());

        // Execute
        var result = await _sut.GetConcertImportPlan(pageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("United States", result.CountryName);
        Assert.Equal("Phoenix", result.CityName);
        Assert.Equal("AZ", result.StateName);
        Assert.Equal("Cricket Pavilion", result.VenueName);
        Assert.Equal("Projekt Revolution 2004", result.TourName);
        Assert.Equal(new DateTimeOffset(2004, 9, 1, 20, 0, 0, TimeSpan.Zero), result.PostedStartTime);
        Assert.NotNull(result.ConcertType);
        Assert.Equal((uint)1, result.ConcertType.Id);
        Assert.Equal("Linkin Park Show", result.ConcertType.Name);

        var foundCountry = Assert.Single(result.FoundCountries);
        Assert.Equal("USA", foundCountry.IsoCode);
        Assert.Equal("United States", foundCountry.Name);

        var foundState = Assert.Single(result.FoundStates);
        Assert.Equal("AZ", foundState.Code);

        var foundCity = Assert.Single(result.FoundCities);
        Assert.Equal((uint)10, foundCity.Id);
        Assert.Equal("Phoenix", foundCity.Name);

        var foundVenue = Assert.Single(result.FoundVenues);
        Assert.Equal((uint)50, foundVenue.Id);

        var foundTour = Assert.Single(result.FoundTours);
        Assert.Equal("pr-2004", foundTour.Id);

        Assert.Empty(result.FoundTourLegs);
    }

    [Fact]
    public async Task GetConcertImportPlan_WhenPageUrlIsArticleTitle_HandlesUrlDecoding()
    {
        var pageUrl = "Live:20250618";
        var mockWikiPage = new WikiPageDto
        {
            Id = 101,
            Title = "Live:20250618",
            Source = SampleBerlinConcertWikitext
        };

        _wikiMediaRepository.GetWikiPageAsync("Live:20250618").Returns(mockWikiPage);

        var countryGer = new CountryDo
        {
            IsoCode = "DEU",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        _countryRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { countryGer }.ToAsyncEnumerable());
        _stateRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<StateDo>());
        _cityRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<CityDo>());
        _venueRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<VenueDo>());
        _tourRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<TourDo>());
        _concertTypeRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { new ConcertTypeDo { Id = 1, Name = "Linkin Park Show" } }.ToAsyncEnumerable());

        var result = await _sut.GetConcertImportPlan(pageUrl);

        Assert.NotNull(result);
        Assert.Equal("Germany", result.CountryName);
        Assert.Equal("Berlin", result.CityName);
        Assert.Null(result.StateName);
        Assert.Equal("Olympiastadion", result.VenueName);
        Assert.Equal("From Zero World Tour", result.TourName);
        Assert.Equal(new DateTimeOffset(2025, 6, 18, 20, 0, 0, TimeSpan.Zero), result.PostedStartTime);
    }

    [Fact]
    public async Task GetConcertImportPlan_WhenWikitextInvalid_ReturnsEmptyPlan()
    {
        var pageUrl = "https://linkinpedia.com/wiki/Live:Invalid";
        var mockWikiPage = new WikiPageDto
        {
            Id = 102,
            Title = "Live:Invalid",
            Source = "Some random content without tourdate"
        };

        _wikiMediaRepository.GetWikiPageAsync("Live:Invalid").Returns(mockWikiPage);
        _countryRepository.QueryAsync(Arg.Any<CancellationToken>()).Returns(AsyncEnumerable.Empty<CountryDo>());
        _stateRepository.QueryAsync(Arg.Any<CancellationToken>()).Returns(AsyncEnumerable.Empty<StateDo>());
        _cityRepository.QueryAsync(Arg.Any<CancellationToken>()).Returns(AsyncEnumerable.Empty<CityDo>());
        _venueRepository.QueryAsync(Arg.Any<CancellationToken>()).Returns(AsyncEnumerable.Empty<VenueDo>());
        _tourRepository.QueryAsync(Arg.Any<CancellationToken>()).Returns(AsyncEnumerable.Empty<TourDo>());
        _concertTypeRepository.QueryAsync(Arg.Any<CancellationToken>()).Returns(AsyncEnumerable.Empty<ConcertTypeDo>());

        var result = await _sut.GetConcertImportPlan(pageUrl);

        Assert.NotNull(result);
        Assert.Empty(result.FoundCountries);
        Assert.Empty(result.FoundStates);
        Assert.Empty(result.FoundCities);
        Assert.Empty(result.FoundVenues);
        Assert.Empty(result.FoundTours);
        Assert.Empty(result.FoundTourLegs);
    }

    [Fact]
    public async Task GetConcertImportPlan_WhenFestivalShowType_MatchesFestivalConcertType()
    {
        var pageUrl = "Live:20110820";
        var festivalWikitext = """
            {{Tourdate
            | ShowType = festival
            | Last show = 2011.08.18
            | Artist = Linkin Park
            | Next show = 2011.08.23
            | Year = 2011
            | Month = August
            | Day = 20
            | Country = Austria
            | City = St. Pölten
            | Venue = Green Park
            | Tour = A Thousand Suns European Tour
            }}
            """;

        var mockWikiPage = new WikiPageDto
        {
            Id = 103,
            Title = "Live:20110820",
            Source = festivalWikitext
        };

        _wikiMediaRepository.GetWikiPageAsync("Live:20110820").Returns(mockWikiPage);

        var countryAut = new CountryDo
        {
            IsoCode = "AUT",
            Name = "Austria",
            NativeName = "Österreich"
        };
        _countryRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { countryAut }.ToAsyncEnumerable());
        _stateRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<StateDo>());

        var cityStPoelten = new CityDo
        {
            Id = 20,
            CountryCode = "AUT",
            Name = "St. Pölten",
            NativeName = "Sankt Pölten",
            Country = countryAut
        };
        _cityRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { cityStPoelten }.ToAsyncEnumerable());

        var venueGreenPark = new VenueDo
        {
            Id = 60,
            CountryCode = "AUT",
            CityId = 20,
            CurrentName = "Green Park",
            TimeZone = "Europe/Vienna",
            Country = countryAut,
            City = cityStPoelten
        };
        _venueRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { venueGreenPark }.ToAsyncEnumerable());

        var tour = new TourDo
        {
            Id = "ats-eu-2011",
            Name = "A Thousand Suns European Tour",
            Legs = []
        };
        _tourRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { tour }.ToAsyncEnumerable());

        var concertTypeLp = new ConcertTypeDo
        {
            Id = 1,
            Name = "Linkin Park Show"
        };
        var concertTypeFestival = new ConcertTypeDo
        {
            Id = 2,
            Name = "Festival"
        };
        _concertTypeRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { concertTypeLp, concertTypeFestival }.ToAsyncEnumerable());

        var result = await _sut.GetConcertImportPlan(pageUrl);

        Assert.NotNull(result);
        Assert.NotNull(result.ConcertType);
        Assert.Equal((uint)2, result.ConcertType.Id);
        Assert.Equal("Festival", result.ConcertType.Name);
        Assert.Equal("Austria", result.CountryName);
        Assert.Equal("St. Pölten", result.CityName);
        Assert.Equal("Green Park", result.VenueName);
        Assert.Single(result.FoundCountries);
        Assert.Single(result.FoundCities);
        Assert.Single(result.FoundVenues);
        Assert.Single(result.FoundTours);
    }
    
    [Fact]
    public async Task GetConcertImportPlan_WhenCancelledShowType_MatchesCancelledStatus()
    {
        var pageUrl = "Live:20171015";
        var festivalWikitext = """
            {{Tourdate
            |Last show=2017.10.14
            |Artist=Linkin Park
            |Next show=2017.10.17
            |Year=2017
            |Month=October
            |Day=15
            |ShowType=cancelled
            |Country=Canada
            |City=Vancouver, BC
            |Venue=Rogers Arena
            |Venue Type=Arena 
            |Venue Website=https://rogersarena.com/
            |Tour=One More Light North American Tour
            |Other artists=Snoop Dogg
            }}
            """;

        var mockWikiPage = new WikiPageDto
        {
            Id = 103,
            Title = "Live:20171015",
            Source = festivalWikitext
        };

        _wikiMediaRepository.GetWikiPageAsync("Live:20171015").Returns(mockWikiPage);

        var countryCanada = new CountryDo
        {
            IsoCode = "CAN",
            Name = "Canada",
            NativeName = "Canada"
        };
        _countryRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { countryCanada }.ToAsyncEnumerable());
        _stateRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(AsyncEnumerable.Empty<StateDo>());

        var cityVancouver = new CityDo
        {
            Id = 20,
            CountryCode = "AUT",
            Name = "Vancouver",
            NativeName = "Vancouver",
            Country = countryCanada
        };
        _cityRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { cityVancouver }.ToAsyncEnumerable());

        var tour = new TourDo
        {
            Id = "oml-na-2017",
            Name = "One More Light North American Tour",
            Legs = []
        };
        _tourRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { tour }.ToAsyncEnumerable());

        var concertTypeLp = new ConcertTypeDo
        {
            Id = 1,
            Name = "Linkin Park Show"
        };
        var concertTypeFestival = new ConcertTypeDo
        {
            Id = 2,
            Name = "Festival"
        };
        _concertTypeRepository.QueryAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { concertTypeLp, concertTypeFestival }.ToAsyncEnumerable());

        var result = await _sut.GetConcertImportPlan(pageUrl);

        Assert.NotNull(result);
        Assert.Equal(ConcertDto.ConcertStatusValue.Cancelled, result.ConcertStatus);
        Assert.NotNull(result.ConcertType);
        Assert.Equal((uint)2, result.ConcertType.Id);
        Assert.Equal("Linkin Park", result.ConcertType.Name);
        Assert.Equal("Canada", result.CountryName);
        Assert.Equal("Vancouver", result.CityName);
        Assert.Equal("Rogers Arena", result.VenueName);
        Assert.Single(result.FoundCountries);
        Assert.Single(result.FoundCities);
        Assert.Single(result.FoundVenues);
        Assert.Single(result.FoundTours);
    }

    [Fact]
    public async Task GetImportStatusList_NoneImported()
    {
        // Set up mock data
        var page1 = new LinkinpediaConcertListEntryBo
        {
            WikiPageId = "Live:20240905",
            ShowType = "main",
            Country = "USA",
            City = "Los Angeles",
            DateString = "2024-09-05",
            State = "California",
            Province = "",
            UkCountry = "",
            Venue = "",
            Tour = "",
            TourLeg = ""
        };
        
        var page2 = new LinkinpediaConcertListEntryBo
        {
            WikiPageId = "Live:20240922",
            ShowType = "main",
            Country = "Germany",
            City = "Hamburg",
            DateString = "2024-09-22",
            State = "",
            Province = "",
            UkCountry = "",
            Venue = "Barclay's Arena",
            Tour = "From Zero World Tour",
            TourLeg = ""
        };
        
        var mockQueryResult = new List<LinkinpediaConcertListEntryBo> { page1, page2 };

        _wikiMediaRepository
            .RunCargoQueryAsync<LinkinpediaConcertListEntryBo>(Arg.Any<string[]>(), Arg.Any<string[]>(), Arg.Any<CargoQueryWhereClause[]>(), Arg.Any<string[]>(), Arg.Is(100), Arg.Any<CancellationToken>())
            .Returns(mockQueryResult.Select(r => new CargoQueryResponseItemDto<LinkinpediaConcertListEntryBo> { Value = r }).ToAsyncEnumerable());
        
        // run the test
        var result = await _sut
            .GetImportStatusList()
            .ToArrayAsync();
        Assert.Equal(2, result.Length);

        var status1 = result.First(r => r.WikiPageId == page1.WikiPageId);
        Assert.Equal(page1.WikiPageId, status1.WikiPageId);
        Assert.Equal("2024-09-05, Los Angeles, USA", status1.ConcertTitle);
        Assert.Equal(ConcertImportStatusBo.Status.NotImported, status1.ImportStatus);
        
        var status2 = result.First(r => r.WikiPageId == page2.WikiPageId);
        Assert.Equal(page2.WikiPageId, status2.WikiPageId);
        Assert.Equal("2024-09-22, Barclay's Arena, Hamburg, Germany", status2.ConcertTitle);
        Assert.Equal(ConcertImportStatusBo.Status.NotImported, status2.ImportStatus);
        
        // verify mock calls
        _concertRepository.GetConcertsByWikiPageId(Arg.Is<string>(p => p == page1.WikiPageId)).Received(1);
        _concertRepository.GetConcertsByWikiPageId(Arg.Is<string>(p => p == page2.WikiPageId)).Received(1);
        _concertRepository.GetConcertsByWikiPageId(Arg.Any<string>()).DidNotReceive();
    }
    
    [Fact]
    public async Task GetImportStatusList_OneImportedWithoutSetlist()
    {
        // Set up mock data
        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };

        var cityHamburg = new CityDo
        {
            CountryCode = "GER",
            Name = "Hamburg",
            NativeName = "Hamburg",
            Country = countryGer,
        };
        
        var page1 = new LinkinpediaConcertListEntryBo
        {
            WikiPageId = "Live:20240905",
            ShowType = "main",
            Country = "USA",
            City = "Los Angeles",
            DateString = "2024-09-05",
            State = "California",
            Province = "",
            UkCountry = "",
            Venue = "",
            Tour = "",
            TourLeg = ""
        };
        
        var page2 = new LinkinpediaConcertListEntryBo
        {
            WikiPageId = "Live:20240922",
            ShowType = "main",
            Country = "Germany",
            City = "Hamburg",
            DateString = "2024-09-22",
            State = "",
            Province = "",
            UkCountry = "",
            Venue = "Barclay's Arena",
            Tour = "From Zero World Tour",
            TourLeg = ""
        };
        
        var existingConcert1 = new ConcertDo
        {
            Id = "concert1",
            Type = new ConcertTypeDo { Id = 1, Name = "Linkin Park Show" },
            LinkinpediaUrl = null,
        };

        var existingConcert2 = new ConcertDo
        {
            Id = "concert2",
            Type = new ConcertTypeDo { Id = 1, Name = "Linkin Park Show" },
            Venue = new VenueDo
            {
                CountryCode = "GER",
                CurrentName = "Barclay's Arena",
                TimeZone = "Europe/Berlin",
                City = cityHamburg,
                Country = countryGer,
            },
            LinkinpediaUrl = $"https://wiki/{page2.WikiPageId}",
        };
        
        var mockQueryResult = new List<LinkinpediaConcertListEntryBo> { page1, page2 };

        _wikiMediaRepository
            .RunCargoQueryAsync<LinkinpediaConcertListEntryBo>(Arg.Any<string[]>(), Arg.Any<string[]>(), Arg.Any<CargoQueryWhereClause[]>(), Arg.Any<string[]>(), Arg.Is(100), Arg.Any<CancellationToken>())
            .Returns(mockQueryResult.Select(r => new CargoQueryResponseItemDto<LinkinpediaConcertListEntryBo> { Value = r }).ToAsyncEnumerable());

        var concertQueryResult = new List<ConcertDo> { existingConcert1, existingConcert2 };
        _concertRepository.FindAllWithReferencesAsync(Arg.Any<CancellationToken>())
            .Returns(concertQueryResult.ToAsyncEnumerable());
        
        // run the test
        var result = await _sut
            .GetImportStatusList()
            .ToArrayAsync();
        Assert.Equal(2, result.Length);

        var status1 = result.First(r => r.WikiPageId == page1.WikiPageId);
        Assert.Equal(page1.WikiPageId, status1.WikiPageId);
        Assert.Equal(ConcertImportStatusBo.Status.NotImported, status1.ImportStatus);
        
        var status2 = result.First(r => r.WikiPageId == page2.WikiPageId);
        Assert.Equal(page2.WikiPageId, status2.WikiPageId);
        Assert.Equal(ConcertImportStatusBo.Status.Imported, status2.ImportStatus);
        
        // verify mock calls
        _concertRepository.GetConcertsByWikiPageId(Arg.Any<string>()).DidNotReceive();
        _concertRepository.FindAllWithReferencesAsync(Arg.Any<CancellationToken>())
            .Received(1);
    }
}
