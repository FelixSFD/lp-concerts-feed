using Database.Users.DataObjects;
using Database.Users.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Users.Tests.Repositories;

/// <summary>Integration tests for ownership-aware subscription queries and mutations.</summary>
public class SqlCalendarSubscriptionRepositoryTest : UsersDbIntegrationTestsBase
{
    /// <summary>Verifies access, settings updates, claiming, and deletion for different callers.</summary>
    [Fact]
    public async Task AccessAndConditionalWritesEnforceOwnership()
    {
        var repo = new SqlCalendarSubscriptionRepository(DbContext);
        var owner = new UserDo { Id = "owner", Username = "owner" };
        var other = new UserDo { Id = "other", Username = "other" };
        DbContext.Users.AddRange(owner, other);
        var anonymous = new CalendarSubscriptionDo();
        var owned = new CalendarSubscriptionDo { UserId = owner.Id };
        repo.Add(anonymous);
        repo.Add(owned);
        await repo.SaveChangesAsync();

        Assert.NotNull(await repo.GetAccessibleAsync(anonymous.Id, null));
        Assert.NotNull(await repo.GetAccessibleAsync(anonymous.Id, other.Id));
        Assert.Null(await repo.GetAccessibleAsync(owned.Id, null));
        Assert.Null(await repo.GetAccessibleAsync(owned.Id, other.Id));
        Assert.Null(await repo.GetAccessibleAsync(owned.Id, "OWNER"));
        Assert.NotNull(await repo.GetAccessibleAsync(owned.Id, owner.Id));
        Assert.Single(await repo.GetForUserAsync(owner.Id));
        Assert.Empty(await repo.GetForUserAsync(other.Id));
        Assert.Empty(await repo.GetForUserAsync("OWNER"));

        Assert.False(await repo.UpdateSettingsAsync(owned.Id, other.Id, true, true));
        Assert.False(await repo.DeleteAccessibleAsync(owned.Id, null));
        Assert.True(await repo.UpdateSettingsAsync(anonymous.Id, null, true, false));
        Assert.True(await repo.UpdateSettingsAsync(anonymous.Id, null, true, false));
        var updated = await repo.GetAccessibleAsync(anonymous.Id, null);
        Assert.True(updated!.IncludeDoorsTime);
        Assert.NotNull(updated.UpdatedAt);

        Assert.True(await repo.ClaimAsync(anonymous.Id, owner.Id));
        Assert.False(await repo.ClaimAsync(anonymous.Id, other.Id));
        Assert.False(await repo.UpdateSettingsAsync(anonymous.Id, null, false, true));
        Assert.False(await repo.DeleteAccessibleAsync(anonymous.Id, other.Id));
        Assert.Null(await repo.GetAccessibleAsync(anonymous.Id, null));
        Assert.True(await repo.DeleteAccessibleAsync(anonymous.Id, owner.Id));
        Assert.True(await repo.DeleteAccessibleAsync(owned.Id, owner.Id));
    }

    /// <summary>Verifies that concurrent claims assign exactly one owner.</summary>
    [Fact]
    public async Task ConcurrentClaimsHaveExactlyOneWinner()
    {
        DbContext.Users.AddRange(
            new UserDo { Id = "first", Username = "first" },
            new UserDo { Id = "second", Username = "second" });
        var subscription = new CalendarSubscriptionDo();
        DbContext.CalendarSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseMySQL(DbContext.Database.GetConnectionString()!)
            .Options;
        await using var firstContext = new UsersDbContext(options);
        await using var secondContext = new UsersDbContext(options);
        var firstRepo = new SqlCalendarSubscriptionRepository(firstContext);
        var secondRepo = new SqlCalendarSubscriptionRepository(secondContext);

        var results = await Task.WhenAll(
            firstRepo.ClaimAsync(subscription.Id, "first"),
            secondRepo.ClaimAsync(subscription.Id, "second"));

        Assert.Single(results.Where(result => result));
        var winner = results[0] ? "first" : "second";
        var loser = results[0] ? "second" : "first";
        var claimed = await repoForVerification().GetAccessibleAsync(subscription.Id, winner);
        Assert.Equal(winner, claimed!.UserId);
        Assert.NotNull(claimed.UpdatedAt);
        Assert.False(await repoForVerification().UpdateSettingsAsync(subscription.Id, loser, true, true));
        Assert.False(await repoForVerification().DeleteAccessibleAsync(subscription.Id, null));

        SqlCalendarSubscriptionRepository repoForVerification() => new(DbContext);
    }

    /// <summary>Verifies that concurrent anonymous mutations cannot bypass a successful claim.</summary>
    [Fact]
    public async Task ConcurrentClaimAndAnonymousMutationCannotBypassOwnership()
    {
        DbContext.Users.Add(new UserDo { Id = "owner", Username = "owner" });
        var updateTarget = new CalendarSubscriptionDo();
        var deleteTarget = new CalendarSubscriptionDo();
        DbContext.CalendarSubscriptions.AddRange(updateTarget, deleteTarget);
        await DbContext.SaveChangesAsync();

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseMySQL(DbContext.Database.GetConnectionString()!)
            .Options;
        await using var claimContext = new UsersDbContext(options);
        await using var mutationContext = new UsersDbContext(options);
        var claimant = new SqlCalendarSubscriptionRepository(claimContext);
        var anonymous = new SqlCalendarSubscriptionRepository(mutationContext);

        var updateResults = await Task.WhenAll(
            claimant.ClaimAsync(updateTarget.Id, "owner"),
            anonymous.UpdateSettingsAsync(updateTarget.Id, null, true, false));
        Assert.True(updateResults[0]);
        var updated = await claimant.GetAccessibleAsync(updateTarget.Id, "owner");
        Assert.Equal(updateResults[1], updated!.IncludeDoorsTime);
        Assert.False(await anonymous.UpdateSettingsAsync(updateTarget.Id, null, false, false));

        var deleteResults = await Task.WhenAll(
            claimant.ClaimAsync(deleteTarget.Id, "owner"),
            anonymous.DeleteAccessibleAsync(deleteTarget.Id, null));
        Assert.NotEqual(deleteResults[0], deleteResults[1]);
        if (deleteResults[0])
            Assert.NotNull(await claimant.GetAccessibleAsync(deleteTarget.Id, "owner"));
        else
            Assert.Null(await claimant.GetAccessibleAsync(deleteTarget.Id, "owner"));
    }
}
