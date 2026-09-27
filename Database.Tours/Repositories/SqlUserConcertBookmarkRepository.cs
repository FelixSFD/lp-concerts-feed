using Common.Database.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

/// <inheritdoc />
public class SqlUserConcertBookmarkRepository(ToursDbContext dbContext) : SqlRepositoryBase<UserConcertBookmarkDo>(dbContext, dbContext.ConcertBookmarks), IUserConcertBookmarkRepository
{
    /// <inheritdoc />
    protected override async Task<UserConcertBookmarkDo> LoadReferences(UserConcertBookmarkDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(c => c.Concert)
            .LoadAsync();
        
        return dataObject;
    }

    /// <inheritdoc />
    public async Task<UserConcertBookmarkDo?> GetByUserIdAndConcertIdAsync(string userId, string concertId, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([userId, concertId], cancellationToken);
    }
}