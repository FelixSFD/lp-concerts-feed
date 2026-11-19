using Database.Users.DataObjects;
using Database.Users.Repositories;
using NSubstitute;
using Service.Users.Exceptions;

namespace Service.Users.Tests;

/// <summary>Tests the calendar subscription service's ownership and profile behavior.</summary>
public class CalendarSubscriptionServiceTest
{
    private readonly ICalendarSubscriptionRepository _subscriptions = Substitute.For<ICalendarSubscriptionRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly CalendarSubscriptionService _service;

    /// <summary>Initializes the service with substitute repositories.</summary>
    public CalendarSubscriptionServiceTest()
    {
        _service = new CalendarSubscriptionService(_subscriptions, _users);
    }

    /// <summary>Verifies that every persisted subscription field is mapped to the business object.</summary>
    [Fact]
    public void ToBoMapsSubscriptionFields()
    {
        var dataObject = new CalendarSubscriptionDo
        {
            Id = Guid.NewGuid().ToString(),
            UserId = "owner",
            IncludeDoorsTime = true,
            IncludeLpStageTime = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var result = dataObject.ToBo();

        Assert.Equal(dataObject.Id, result.Id);
        Assert.Equal(dataObject.UserId, result.UserId);
        Assert.Equal(dataObject.IncludeDoorsTime, result.IncludeDoorsTime);
        Assert.Equal(dataObject.IncludeLpStageTime, result.IncludeLpStageTime);
        Assert.Equal(dataObject.CreatedAt, result.CreatedAt);
        Assert.Equal(dataObject.UpdatedAt, result.UpdatedAt);
    }

    /// <summary>Verifies generated IDs, caller ownership, and saved settings.</summary>
    /// <param name="userId">Caller ID, or null for an anonymous caller.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("owner")]
    public async Task CreateGeneratesUuidAndSetsOwnerFromCaller(string? userId)
    {
        if (userId is not null)
            _users.GetByPrimaryKeyAsync(userId, Arg.Any<CancellationToken>())
                .Returns(new UserDo { Id = userId, Username = "owner" });
        CalendarSubscriptionDo? saved = null;
        _subscriptions.When(x => x.Add(Arg.Any<CalendarSubscriptionDo>()))
            .Do(call => saved = call.Arg<CalendarSubscriptionDo>());

        var result = await _service.CreateAsync(userId, true, false);

        Assert.True(Guid.TryParse(result.Id, out _));
        Assert.Equal(userId, result.UserId);
        Assert.True(result.IncludeDoorsTime);
        Assert.False(result.IncludeLpStageTime);
        Assert.Equal(result.Id, saved!.Id);
        await _subscriptions.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies that owned creation and claiming require a local profile.</summary>
    [Fact]
    public async Task MissingProfilePreventsCreateAndClaim()
    {
        await Assert.ThrowsAsync<MissingUserProfileException>(() => _service.CreateAsync("missing", true, true));
        await Assert.ThrowsAsync<MissingUserProfileException>(() => _service.ClaimAsync("id", "missing"));
        _subscriptions.DidNotReceive().Add(Arg.Any<CalendarSubscriptionDo>());
        await _subscriptions.DidNotReceive().ClaimAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Verifies idempotent claims and rejection of subscriptions owned by another user.</summary>
    [Fact]
    public async Task ClaimIsIdempotentForOwnerAndDoesNotReturnOtherOwners()
    {
        _users.GetByPrimaryKeyAsync("owner", Arg.Any<CancellationToken>())
            .Returns(new UserDo { Id = "owner", Username = "owner" });
        var subscription = new CalendarSubscriptionDo { UserId = "owner" };
        _subscriptions.GetAccessibleAsync(subscription.Id, "owner", Arg.Any<CancellationToken>())
            .Returns(subscription);

        var claimed = await _service.ClaimAsync(subscription.Id, "owner");
        Assert.Equal("owner", claimed!.UserId);

        subscription.UserId = "another";
        Assert.Null(await _service.ClaimAsync(subscription.Id, "owner"));
    }

    /// <summary>Verifies that service operations pass identity and settings to the repository.</summary>
    [Fact]
    public async Task OperationsForwardCallerIdentityAndSettings()
    {
        await _service.GetAsync("id", "owner");
        await _service.GetForUserAsync("owner");
        await _service.UpdateAsync("id", "owner", true, false);
        await _service.DeleteAsync("id", "owner");

        await _subscriptions.Received(1).GetAccessibleAsync("id", "owner", Arg.Any<CancellationToken>());
        await _subscriptions.Received(1).GetForUserAsync("owner", Arg.Any<CancellationToken>());
        await _subscriptions.Received(1).UpdateSettingsAsync("id", "owner", true, false, Arg.Any<CancellationToken>());
        await _subscriptions.Received(1).DeleteAccessibleAsync("id", "owner", Arg.Any<CancellationToken>());
    }
}
