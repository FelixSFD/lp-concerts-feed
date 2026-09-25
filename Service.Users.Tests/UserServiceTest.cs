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
    public async Task CreateUserAsync(string mockUsername, string mockId)
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
}