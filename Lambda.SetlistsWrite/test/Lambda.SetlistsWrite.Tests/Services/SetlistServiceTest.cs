using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Lambda.SetlistsWrite.Services;
using LPCalendar.DataStructure.Setlists;
using NSubstitute;
using Xunit;

namespace Lambda.SetlistsWrite.Tests.Services;

public class SetlistServiceTest
{
    private readonly ISetlistRepository _setlistRepository;
    private readonly ISetlistEntryRepository _setlistEntryRepository;
    private readonly IConcertRepository _concertRepository;
    private readonly ISongRepository _songRepository;
    private readonly ISetlistActRepository _actRepository;
    private readonly ILambdaLogger _logger = new TestLambdaLogger();
    private readonly SetlistService _setlistService;

    public SetlistServiceTest()
    {
        _setlistRepository = Substitute.For<ISetlistRepository>();
        _setlistEntryRepository = Substitute.For<ISetlistEntryRepository>();
        _concertRepository = Substitute.For<IConcertRepository>();
        _songRepository = Substitute.For<ISongRepository>();
        _actRepository = Substitute.For<ISetlistActRepository>();
        
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository, _actRepository, _logger);
    }

    [Theory]
    [InlineData("testconcert123", "https://lplive.net")]
    [InlineData("1234567890", null)]
    public async Task CreateSetlistAsync(string concertId, string? linkinpediaUrl)
    {
        // setup mock
        _setlistRepository.Add(Arg.Is<SetlistDo>(setlist => setlist.ConcertId == concertId && setlist.LinkinpediaUrl == linkinpediaUrl && setlist.Id == 0));
        
        // call the service
        var createRequest = new CreateSetlistRequestDto
        {
            ConcertId = concertId,
            LinkinpediaUrl = linkinpediaUrl
        };
        var response = await _setlistService.CreateSetlistAsync(createRequest);
        
        // validate if repo was called correctly
        _setlistRepository
            .Received(1)
            .Add(Arg.Is<SetlistDo>(setlist => setlist.ConcertId == concertId && setlist.LinkinpediaUrl == linkinpediaUrl && setlist.Id == 0));
        
        // validate result
        Assert.NotNull(response);
        Assert.Equal(createRequest.ConcertId, response.ConcertId);
        Assert.Equal(createRequest.LinkinpediaUrl, response.LinkinpediaUrl);
    }
}