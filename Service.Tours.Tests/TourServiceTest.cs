using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.Exceptions;

namespace Service.Tours.Tests;

public class TourServiceTest
{
    private readonly ITourRepository _tourRepository;
    private readonly TourService _service;

    public TourServiceTest()
    {
        var logger = Substitute.For<ILogger<TourService>>();
        _tourRepository = Substitute.For<ITourRepository>();
        _service = new TourService(_tourRepository, logger);
    }

    [Theory]
    [InlineData("fz-world-tour", "From Zero World Tour")]
    [InlineData("oml-world-tour", "One More Light World Tour")]
    public async Task CreateTour(string tourId, string name)
    {
        var request = new CreateTourRequestDto
        {
            Id = tourId,
            Name = name,
        };

        TourDo? savedTourDo = null;
        _tourRepository
            .When(r => r.Add(Arg.Is<TourDo>(t => t.Id == tourId && t.Name == name)))
            .Do(cb =>
            {
                savedTourDo = cb.Arg<TourDo>();
                savedTourDo.Id = tourId;
            });
        
        // call the service
        var createdTour = await _service.CreateTourAsync(request);
        Assert.NotNull(createdTour);
        Assert.Equal(tourId, createdTour.Id);
        Assert.Equal(name, createdTour.Name);
        Assert.NotNull(savedTourDo);
        Assert.Equal(tourId, savedTourDo.Id);
        Assert.Equal(name, savedTourDo.Name);
        
        // verify mock calls
        _tourRepository
            .Received(1)
            .Add(Arg.Is<TourDo>(t => t.Id == tourId && t.Name == name));
        await _tourRepository
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task GetTourByIdAsync()
    {
        var mockTour = new TourDo
        {
            Id = "fz-world-tour",
            Name = "From Zero World Tour"
        };
        
        // setup mocks
        _tourRepository
            .GetByPrimaryKeyAsync(mockTour.Id)
            .Returns(mockTour);
        
        // call the service
        var foundTour = await _service.GetTourByIdAsync(mockTour.Id);
        Assert.NotNull(foundTour);
        Assert.Equal(mockTour.Id, foundTour.Id);
        Assert.Equal(mockTour.Name, foundTour.Name);
        
        // verify mock calls
        await _tourRepository
            .Received(1)
            .GetByPrimaryKeyAsync(mockTour.Id);
        await _tourRepository
            .DidNotReceive()
            .SaveChangesAsync();
    }
    
    [Fact]
    public async Task DeleteTourByIdAsync()
    {
        var mockTour = new TourDo
        {
            Id = "fz-world-tour",
            Name = "From Zero World Tour"
        };
        
        // setup mocks
        _tourRepository
            .GetByPrimaryKeyAsync(mockTour.Id)
            .Returns(mockTour);
        
        // call the service
        await _service.DeleteTourAsync(mockTour.Id);
        
        // verify mock calls
        await _tourRepository
            .Received(1)
            .GetByPrimaryKeyAsync(mockTour.Id);
        _tourRepository
            .Received(1)
            .Delete(Arg.Is<TourDo>(t => t.Id == mockTour.Id));
        await _tourRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    [Fact]
    public async Task DeleteTourByIdAsync_NotFound()
    {
        // setup mocks
        _tourRepository
            .GetByPrimaryKeyAsync(Arg.Any<string>())
            .Returns((TourDo?)null);
        
        // call the service
        var exception = await Assert.ThrowsAsync<TourNotFoundException>(async () => await _service.DeleteTourAsync("test-tour"));
        Assert.Equal("test-tour", exception.TourId);
        
        // verify mock calls
        await _tourRepository
            .Received(1)
            .GetByPrimaryKeyAsync("test-tour");
        _tourRepository
            .DidNotReceive()
            .Delete(Arg.Any<TourDo>());
        await _tourRepository
            .DidNotReceive()
            .SaveChangesAsync();
    }
}