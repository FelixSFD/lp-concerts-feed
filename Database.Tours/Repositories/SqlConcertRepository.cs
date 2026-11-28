using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Common.Database;
using Common.Database.DataObjects;
using Common.Database.Repositories;
using Common.Database.MySql.Repositories;
using Common.Utils.Diagnostics;
using Database.Tours.DataObjects;
using Database.Tours.Filters;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlConcertRepository(ToursDbContext dbContext, TimeProvider timeProvider) : SingleKeySqlRepositoryBase<ConcertDo, string>(dbContext, dbContext.Concerts), IConcertRepository
{
    private static readonly ActivitySource ActivitySource = ActivitySourceProvider.Default;
    
    /// <summary>Creates a repository using the system clock.</summary>
    public SqlConcertRepository(ToursDbContext dbContext) : this(dbContext, TimeProvider.System) { }

    protected override IReadOnlyDictionary<string, LambdaExpression> SortExpressions { get; } = new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
    {
        ["date"] = (Expression<Func<ConcertDo, DateTime>>)(c => c.PostedStartTime),
        ["venue"] = (Expression<Func<ConcertDo, string>>)(c => c.Venue.CurrentName),
        ["city"] = (Expression<Func<ConcertDo, string>>)(c => c.Venue.City.Name),
        ["country"] = (Expression<Func<ConcertDo, string>>)(c => c.Venue.Country.Name),
        ["tour"] = (Expression<Func<ConcertDo, string>>)(c => c.Tour!.Name),
        ["type"] = (Expression<Func<ConcertDo, string>>)(c => c.Type.Name),
        ["customTitle"] = (Expression<Func<ConcertDo, string>>)(c => c.CustomTitle!),
        ["status"] = (Expression<Func<ConcertDo, ConcertDo.ConcertStatus>>)(c => c.Status),
        ["lastUpdated"] = (Expression<Func<ConcertDo, DateTimeOffset?>>)(c => c.UpdatedAt ?? c.CreatedAt),
    };
    
    protected override async Task<ConcertDo> LoadReferences(ConcertDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(c => c.Type)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(c => c.Tour)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(c => c.TourLeg)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(c => c.Venue)
            .LoadAsync();
        
        await Context.Entry(dataObject.Venue)
            .Reference(v => v.City)
            .LoadAsync();
        await Context.Entry(dataObject.Venue)
            .Reference(v => v.State)
            .LoadAsync();
        await Context.Entry(dataObject.Venue)
            .Reference(v => v.Country)
            .LoadAsync();
        await Context.Entry(dataObject.Venue.City)
            .Reference(vc => vc.Country)
            .LoadAsync();
        await Context.Entry(dataObject.Venue.City)
            .Reference(vc => vc.State)
            .LoadAsync();

        var venueState = dataObject.Venue.State;
        if (venueState is not null)
        {
            await Context.Entry(venueState)
                .Reference(vs => vs.Country)
                .LoadAsync();
        }
        
        return dataObject;
    }

    private static IQueryable<ConcertDo> IncludeAllReferences(IQueryable<ConcertDo> queryable) =>
        queryable.Include(c => c.Type)
            .Include(c => c.Venue)
            .Include(c => c.Venue.City)
            .Include(c => c.Venue.Country)
            .Include(c => c.Venue.State)
            .Include(c => c.Tour)
            .Include(c => c.TourLeg);
    
    private static IQueryable<ConcertDo> IncludeVenueDetailsReferences(IQueryable<ConcertDo> queryable) =>
        IncludeAllReferences(queryable)
            .Include(c => c.Venue.City.Country)
            .Include(c => c.Venue.City.State)
            .Include(c => c.Venue.PreviousNames);
    
    /// <inheritdoc/>
    protected override IQueryable<ConcertDo> DefaultQueryConfiguration(IQueryable<ConcertDo> queryable) => IncludeAllReferences(queryable);
    
    /// <inheritdoc/>
    public async Task<PaginatedQueryResult<ConcertDo>> GetConcertsAsync(CancellationToken token, ConcertFilter? filter = null,
        IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false)
    {
        paginationParams ??= new PaginationParams(0, 100);
        return await InternalFindPaginatedAsync(filter, IncludeAllReferences, orderBy, paginationParams, includeDeleted, token);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, ConcertFilter? filter = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false)
    {
        paginationParams ??= new PaginationParams(0, 100);
        return InternalFindAsync(filter, IncludeAllReferences, orderBy, paginationParams, includeDeleted, token);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetOnThisDay(CancellationToken token, int month, int day, IEnumerable<SortDescriptor>? orderBy = null)
    {
        return InternalFindAsync(c => c.PostedStartTime.Month == month && c.PostedStartTime.Day == day, IncludeAllReferences, orderBy);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetConcertsByWikiPageId(string wikiPageId)
    {
        return FindDeletableAsync(
            c => c.LinkinpediaUrl != null && EF.Functions.Like(c.LinkinpediaUrl, $"%{wikiPageId}"),
            IncludeAllReferences);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ConcertDo> FindAllWithReferencesAsync([EnumeratorCancellation] CancellationToken token)
    {
        const uint pageSize = 100;
        uint skip = 0;
        var orderBy = new[] { new SortDescriptor("date") };

        while (true)
        {
            token.ThrowIfCancellationRequested();
            var pageCount = 0;
            var page = FindDeletableAsync(
                concert => concert.DeletedAt == null,
                IncludeAllReferences,
                orderBy,
                new PaginationParams(skip, pageSize));

            await foreach (var concert in page.WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();
                pageCount++;
                yield return concert;
            }

            if (pageCount == 0)
                yield break;

            skip = checked(skip + (uint)pageCount);
        }
    }

    public IAsyncEnumerable<ConcertDo> GetUpcomingBookmarkedConcertsForAsync(string userId,
        UserConcertBookmarkDo.BookmarkStatus status, CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var query = DbSet.AsQueryable();
        query = IncludeAllReferences(query);
        return query
            .Where(c => c.Bookmarks.Any(b => b.UserId == userId && b.Status == status))
            .Where(c =>
                (c.MainStageTime ?? c.PostedStartTime)
                .AddMinutes(c.ExpectedSetDurationMinutes == 0 ? 120 : (int)c.ExpectedSetDurationMinutes) > nowUtc)
            .OrderBy(c => c.PostedStartTime)
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetAttendedConcertsForAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("userId", userId);
        
        var query = DbSet
            .AsQueryable();
        query = IncludeVenueDetailsReferences(query);
        
        return query
            .Where(c => c.DeletedAt == null)
            .Where(c => c.PostedStartTime < DateTime.UtcNow)
            .Where(c => c.Bookmarks.Any(b => b.UserId == userId && b.Status == UserConcertBookmarkDo.BookmarkStatus.Attending))
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ConcertDo> GetConcertsForStatsAsync(string? tourId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("tourId", tourId);
        
        var query = IncludeVenueDetailsReferences(DbSet.AsNoTracking())
            .Where(c => c.DeletedAt == null);
        if (tourId is not null)
            query = query.Where(c => c.TourId == tourId);

        await foreach (var concert in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
            yield return concert;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetByIds(IEnumerable<string> ids)
    {
        return DbSet
            .Where(c => ids.Contains(c.Id))
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async Task<DateTimeOffset?> GetLatestChangeAsync(CancellationToken cancellationToken = default)
    {
        var createdAt = await DbSet.MaxAsync(c => (DateTimeOffset?)c.CreatedAt, cancellationToken);
        var updatedAt = await DbSet.MaxAsync(c => c.UpdatedAt, cancellationToken);
        var deletedAt = await DbSet.MaxAsync(c => c.DeletedAt, cancellationToken);

        return new[] { createdAt, updatedAt, deletedAt }.Max();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetChangedAfter(DateTimeOffset timestamp)
    {
        var query = DbSet.AsQueryable();
        query = IncludeAllReferences(query);
        return query
            .Where(c => c.UpdatedAt > timestamp || c.CreatedAt > timestamp)
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertDo> GetDeletedAfter(DateTimeOffset timestamp)
    {
        var query = DbSet.AsQueryable();
        query = IncludeAllReferences(query);
        return query
            .Where(c => c.CreatedAt <= timestamp && c.DeletedAt > timestamp)
            .ToAsyncEnumerable();
    }
}