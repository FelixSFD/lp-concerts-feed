using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.Exceptions;

namespace Service.Tours.Tests;

public class ConcertServiceTest
{
    private readonly IConcertRepository _concertRepository;
    private readonly IConcertTypeRepository _concertTypeRepository;
    private readonly ConcertService _service;

    public ConcertServiceTest()
    {
        var logger = Substitute.For<ILogger<ConcertService>>();
        _concertRepository = Substitute.For<IConcertRepository>();
        _concertTypeRepository = Substitute.For<IConcertTypeRepository>();
        _service = new ConcertService(_concertRepository, _concertTypeRepository, logger);
    }

    [Theory]
    [InlineData("Linkin Park", 1u)]
    [InlineData("Festival", 1337u)]
    public async Task CreateConcertType(string name, uint mockId)
    {
        var request = new CreateConcertTypeRequestDto
        {
            Name = name,
        };

        ConcertTypeDo? savedConcertType = null;
        _concertTypeRepository
            .When(r => r.Add(Arg.Is<ConcertTypeDo>(t => t.Id == 0 && t.Name == name)))
            .Do(cb =>
            {
                savedConcertType = cb.Arg<ConcertTypeDo>();
                savedConcertType.Id = mockId;
            });
        
        // call the service
        var createdType = await _service.CreateConcertTypeAsync(request);
        Assert.NotNull(createdType);
        Assert.Equal(mockId, createdType.Id);
        Assert.Equal(name, createdType.Name);
        Assert.NotNull(savedConcertType);
        Assert.Equal(mockId, savedConcertType.Id);
        Assert.Equal(name, savedConcertType.Name);
        
        // verify mock calls
        _concertTypeRepository
            .Received(1)
            .Add(Arg.Is<ConcertTypeDo>(t => t.Name == name));
        await _concertTypeRepository
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task GetConcertTypeAsync()
    {
        var mockType = new ConcertTypeDo
        {
            Id = 1337u,
            Name = "Linkin Park",
        };
        
        _concertTypeRepository
            .GetByPrimaryKeyAsync(Arg.Is<uint>(id => id == mockType.Id))
            .Returns(mockType);

        var result = await _service.GetConcertTypeAsync(mockType.Id);
        Assert.NotNull(result);
        Assert.Equal(mockType.Id, result.Id);
        Assert.Equal(mockType.Name, result.Name);
        
        await _concertTypeRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is<uint>(id => id == mockType.Id));
    }
    
    [Fact]
    public async Task GetConcertTypeAsync_NotFound()
    {
        _concertTypeRepository
            .GetByPrimaryKeyAsync(Arg.Any<uint>())
            .Returns((ConcertTypeDo?)null);

        var exception = await Assert.ThrowsAsync<ConcertTypeNotFoundException>(async () 
            => await _service.GetConcertTypeAsync(404u));
        Assert.Equal(404u, exception.TypeId);
        
        await _concertTypeRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Any<uint>());
    }
    
    [Fact]
    public async Task GetConcertTypesAsync()
    {
        var mockType = new ConcertTypeDo
        {
            Id = 1337u,
            Name = "Linkin Park",
        };

        ConcertTypeDo[] mockTypes = [mockType];

        _concertTypeRepository
            .QueryAsync(Arg.Any<CancellationToken>())
            .Returns(mockTypes.ToAsyncEnumerable());

        var results = await _service.GetConcertTypesAsync(CancellationToken.None).ToArrayAsync();
        Assert.NotNull(results);
        var result = Assert.Single(results);
        Assert.Equal(mockType.Id, result.Id);
        Assert.Equal(mockType.Name, result.Name);
        
        _concertTypeRepository
            .Received(1)
            .QueryAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateConcertAsync()
    {
        var mockConcertId = Guid.NewGuid().ToString();
        var mockConcertType = new ConcertTypeDo
        {
            Id = 1337u,
            Name = "Linkin Park",
        };
        var mockTour = new TourDo
        {
            Id = "fz-world-tour",
            Name = "From Zero World Tour",
            Legs = [],
        };
        var mockTourLegEu = new TourLegDo
        {
            Id = "europe-2025",
            TourId = mockTour.Id,
            Tour = mockTour,
            Name = "European Tour 2025",
        };
        mockTour.Legs.Add(mockTourLegEu);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };
        var cityMuc = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            Name = "Munich",
            NativeName = "München",
        };
        var mockVenue = new VenueDo
        {
            Id = 1900u,
            CountryCode = countryGer.IsoCode,
            CityId = cityMuc.Id,
            City = cityMuc,
            CurrentName = "Allianz Arena",
            TimeZone = "Europe/Berlin",
            Latitude = 12.34m,
            Longitude = 0.123m,
            Country = countryGer,
        };
        
        // setup mocks
        ConcertDo? savedConcert = null;
        _concertRepository
            .When(r => r.Add(Arg.Any<ConcertDo>()))
            .Do(cb =>
            {
                savedConcert = cb.Arg<ConcertDo>();
                savedConcert.Id = mockConcertId;
            });
        
        // call the service
        var request = new CreateConcertRequestDto
        {
            TourId = mockTour.Id,
            TourLegId = mockTourLegEu.Id,
            ConcertTypeId = mockConcertType.Id,
            PostedStartTime = new DateTimeOffset(2026, 6, 30, 20, 0, 0, TimeSpan.FromHours(2)),
            MainStageTime = new DateTime(2026, 6, 30, 20, 25, 0),
            DoorsTime = new DateTime(2026, 6, 30, 16, 0, 0),
            LpuEarlyEntryTime = new DateTime(2026, 6, 30, 15, 30, 0),
            LpuEarlyEntryConfirmed = true,
            VenueId = mockVenue.Id,
            Status = ConcertDto.ConcertStatusValue.Past,
            ExpectedSetDurationMinutes = 120,
            CustomTitle = "Final Show of the tour",
            ScheduleImageFile = "test.jpg",
        };

        var createdConcert = await _service.CreateConcertAsync(request);
        Assert.NotNull(createdConcert);
        Assert.NotNull(savedConcert);
        Assert.Equal(mockConcertId, createdConcert.Id);
        Assert.Equal(mockTour.Id, createdConcert.TourId);
        Assert.Equal(mockTourLegEu.Id, createdConcert.TourLegId);
        Assert.Equal(mockConcertType.Id, createdConcert.ConcertTypeId);
        Assert.Equal(mockVenue.Id, createdConcert.VenueId);
        Assert.Equal(request.CustomTitle, createdConcert.CustomTitle);
        Assert.Equal(request.ScheduleImageFile, createdConcert.ScheduleImageFile);
        Assert.Equal(request.ExpectedSetDurationMinutes, createdConcert.ExpectedSetDurationMinutes);
        Assert.Equal(request.PostedStartTime, createdConcert.PostedStartTime);
        Assert.Equal(request.MainStageTime, createdConcert.MainStageTime);
        Assert.Equal(request.DoorsTime, createdConcert.DoorsTime);
        Assert.Equal(request.LpuEarlyEntryTime, createdConcert.LpuEarlyEntryTime);
        Assert.Equal(request.LpuEarlyEntryConfirmed, createdConcert.LpuEarlyEntryConfirmed);
        // TODO: fix mapping of status
        //Assert.Equal(request.Status, createdConcert.Status);
        
        // verify mock calls
        _concertRepository
            .Received(1)
            .Add(Arg.Any<ConcertDo>());
        await _concertRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    private static ConcertDo CreateMockConcert()
    {
        var mockConcertId = Guid.NewGuid().ToString();
        var mockConcertType = new ConcertTypeDo
        {
            Id = 1337u,
            Name = "Linkin Park",
        };
        var mockTour = new TourDo
        {
            Id = "fz-world-tour",
            Name = "From Zero World Tour",
            Legs = [],
        };
        var mockTourLegEu = new TourLegDo
        {
            Id = "europe-2025",
            TourId = mockTour.Id,
            Tour = mockTour,
            Name = "European Tour 2025",
        };
        mockTour.Legs.Add(mockTourLegEu);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };
        var cityMuc = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            Name = "Munich",
            NativeName = "München",
        };
        var mockVenue = new VenueDo
        {
            Id = 1900u,
            CountryCode = countryGer.IsoCode,
            CityId = cityMuc.Id,
            City = cityMuc,
            CurrentName = "Allianz Arena",
            TimeZone = "Europe/Berlin",
            Latitude = 12.34m,
            Longitude = 0.123m,
            Country = countryGer,
        };
        
        var mockConcert = new ConcertDo
        {
            Id = mockConcertId,
            TourId = mockTour.Id,
            TourLegId = mockTourLegEu.Id,
            ConcertTypeId = mockConcertType.Id,
            PostedStartTime = new DateTimeOffset(2026, 6, 30, 20, 0, 0, TimeSpan.FromHours(2)),
            MainStageTime = new DateTime(2026, 6, 30, 20, 25, 0),
            DoorsTime = new DateTime(2026, 6, 30, 16, 0, 0),
            LpuEarlyEntryTime = new DateTime(2026, 6, 30, 15, 30, 0),
            LpuEarlyEntryConfirmed = true,
            VenueId = mockVenue.Id,
            Status = ConcertDo.ConcertStatus.Planned,
            ExpectedSetDurationMinutes = 120,
            CustomTitle = "Final Show of the tour",
            ScheduleImageFile = "test.jpg",
        };
        
        return mockConcert;
    }
    
    [Fact]
    public async Task DeleteConcertAsync_MarkDeletedOnly()
    {
        var mockConcert = CreateMockConcert();
        
        // setup mocks
        _concertRepository
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<string>(s => s == mockConcert.Id))
            .Returns(mockConcert);
        
        ConcertDo? savedConcert = null;
        _concertRepository
            .When(r => r.Update(Arg.Any<ConcertDo>()))
            .Do(cb =>
            {
                savedConcert = cb.Arg<ConcertDo>();
            });
        
        // call the service
        await _service.DeleteConcertAsync(mockConcert.Id);
        await Task.Delay(TimeSpan.FromSeconds(3));
        Assert.NotNull(savedConcert);
        Assert.Equal(mockConcert.Id, savedConcert.Id);
        var deletedAt = Assert.NotNull(savedConcert.DeletedAt);
        Assert.True(deletedAt <= DateTimeOffset.UtcNow);
        
        // verify mock calls
        _concertRepository
            .DidNotReceive()
            .Delete(Arg.Any<ConcertDo>());
        _concertRepository
            .Received(1)
            .Update(Arg.Is<ConcertDo>(c => c.Id == mockConcert.Id));
        await _concertRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    [Fact]
    public async Task DeleteConcertAsync_ActuallyRemoveFromDb()
    {
        var mockConcert = CreateMockConcert();
        
        // setup mocks
        _concertRepository
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<string>(s => s == mockConcert.Id))
            .Returns(mockConcert);
        
        // call the service
        await _service.DeleteConcertAsync(mockConcert.Id, true);
        
        // verify mock calls
        _concertRepository
            .Received(1)
            .Delete(Arg.Is<ConcertDo>(c => c.Id == mockConcert.Id));
        _concertRepository
            .DidNotReceive()
            .Update(Arg.Any<ConcertDo>());
        await _concertRepository
            .Received(1)
            .SaveChangesAsync();
    }
}