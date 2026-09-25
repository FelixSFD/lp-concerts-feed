using Database.Users.DataObjects;
using Database.Users.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Service.Users.Tests;

public class UserServiceTest
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _service;

    public UserServiceTest()
    {
        _userRepository = Substitute.For<IUserRepository>();
        var logger = Substitute.For<ILogger<UserService>>();
        _service = new UserService(_userRepository, logger);
    }

    [Theory]
    [InlineData("FelixSFD", "1234")]
    [InlineData("test_user", "6352342")]
    public async Task CreateUserAsync_NoId(string mockUsername, string mockId)
    {
        // setup mocks
        UserDo? savedUser = null;
        _userRepository.When(r => r.Add(Arg.Is<UserDo>(u => u.Username == mockUsername)))
            .Do(cb =>
            {
                savedUser = cb.Arg<UserDo>();
                savedUser.Id = mockId;
            });
        _userRepository
            .GetByPrimaryKeyAsync(Arg.Is(mockId))
            .Returns(_ => Task.FromResult(savedUser));

        // run the test
        await _service.CreateUserAsync(mockUsername);

        // check result
        Assert.NotNull(savedUser);
        Assert.Equal(mockUsername, savedUser.Username);
        Assert.Equal(mockId, savedUser.Id);

        await _userRepository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("FelixSFD", "1234")]
    [InlineData("test_user", "6352342")]
    public async Task CreateUserAsync_PredefinedId(string mockUsername, string mockId)
    {
        // setup mocks
        UserDo? savedUser = null;
        _userRepository.When(r => r.Add(Arg.Is<UserDo>(u => u.Username == mockUsername)))
            .Do(cb => { savedUser = cb.Arg<UserDo>(); });
        _userRepository
            .GetByPrimaryKeyAsync(Arg.Is(mockId))
            .Returns(_ => Task.FromResult(savedUser));

        // run the test
        await _service.CreateUserAsync(mockUsername, mockId);

        // check result
        Assert.NotNull(savedUser);
        Assert.Equal(mockUsername, savedUser.Username);
        Assert.Equal(mockId, savedUser.Id);

        await _userRepository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser()
    {
        var mockUser = new UserDo
        {
            Id = Guid.NewGuid().ToString(),
            Username = "FelixSFD",
        };
        
        _userRepository
            .GetByPrimaryKeyAsync(Arg.Is(mockUser.Id), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<UserDo?>(mockUser));
        
        var result = await _service.GetUserById(mockUser.Id);

        Assert.NotNull(result);
        Assert.Equal(mockUser.Id, result.Id);
        Assert.Equal(mockUser.Username, result.Username);
    }
}