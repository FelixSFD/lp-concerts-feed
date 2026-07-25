using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;
using NSubstitute;

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
        var createdType = await _service.CreateConcertType(request);
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
}