using System.Linq.Expressions;
using Common.Database.MySql.Repositories;
using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlAlbumRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<AlbumDo, uint>(dbContext, dbContext.Albums), IAlbumRepository
{
    protected override IReadOnlyDictionary<string, LambdaExpression> SortExpressions { get; } = new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
    {
        ["title"] = (Expression<Func<AlbumDo, string>>)(c => c.Title),
    };
    
    /// <inheritdoc/>
    protected override Task<AlbumDo> LoadReferences(AlbumDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}