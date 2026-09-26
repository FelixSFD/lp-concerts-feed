using Common.Database;
using Database.Users.DataObjects;
using Database.Users.Filters;
using Database.Users.Repositories;

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
    
    [Fact]
    public async Task FindPaginated_SortByUsername()
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
        var user3 = new UserDo
        {
            Id = Guid.NewGuid().ToString(),
            Username = "nobody"
        };
        
        repo.Add(user1);
        repo.Add(user2);
        repo.Add(user3);
        
        await repo.SaveChangesAsync();
        
        var filter = new UserFilter
        {
            Username = "user"
        };
        
        SortDescriptor[] sort = [
            new("username")
        ];

        var result = await repo.FindPaginatedAsync(filter, sort);
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        
        var retrievedUsers = await result.Results.ToArrayAsync();
        Assert.Equal(2, retrievedUsers.Length);
        AssertUsersEqual(user1, retrievedUsers[0]);
        AssertUsersEqual(user2, retrievedUsers[1]);
        
        repo.Delete(user1);
        repo.Delete(user2);
        repo.Delete(user3);
        
        await repo.SaveChangesAsync();
    }
    
    [Fact]
    public async Task FindPaginated_InvalidSortField()
    {
        var repo = new SqlUserRepository(DbContext);
        
        var filter = new UserFilter();
        
        SortDescriptor[] sort = [
            new("not_valid")
        ];

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await repo.FindPaginatedAsync(filter, sort));
        Assert.Equal("Unknown sort property 'not_valid'.", exception.Message);
    }


    private static void AssertUsersEqual(UserDo expected, UserDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Username, actual.Username);
    }
}