using Common.Database.MySql.Repositories;
using Common.Database.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

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

    /// <inheritdoc />
    public async Task<IList<UserConcertBookmarkDo>> GetByConcertId(string concertId, CancellationToken cancellationToken = default)
    {
        var rawConcert = await dbContext.Concerts.FindAsync([concertId], cancellationToken) ?? throw new ArgumentException($"Concert '{concertId}' does not exist", nameof(concertId));
        await dbContext
            .Entry(rawConcert)
            .Collection(c => c.Bookmarks!)
            .LoadAsync(cancellationToken);
        return rawConcert.Bookmarks?.ToList() ?? [];
    }
}