using Database.Users.DataObjects;
using Database.Users.Repositories;
using Xunit;

namespace Database.Users.Tests.Repositories;

public class SqlUserRepositoryTest : UsersDbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlUserRepository(DbContext);

        var user1 = new UserDo
        {
            Id = Guid.NewGuid().ToString(),
            Username = "user1"
        };
        var user2 = new UserDo
        {
            Id = Guid.NewGuid().ToString(),
            Username = "user2"
        };
        
        repo.Add(user1);
        repo.Add(user2);
        
        await repo.SaveChangesAsync();

        var retrievedUser = await repo.GetByPrimaryKeyAsync(user1.Id);
        Assert.NotNull(retrievedUser);
        AssertUsersEqual(user1, retrievedUser);
        
        repo.Delete(user1);
        
        await repo.SaveChangesAsync();
        
        retrievedUser = await repo.GetByPrimaryKeyAsync(user1.Id);
        Assert.Null(retrievedUser);
        
        repo.Delete(user2);
        
        await repo.SaveChangesAsync();
    }


    private static void AssertUsersEqual(UserDo expected, UserDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Username, actual.Username);
    }
}