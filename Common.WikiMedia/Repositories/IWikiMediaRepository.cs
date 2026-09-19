using Common.WikiMedia.DTOs;

namespace Common.WikiMedia.Repositories;

public interface IWikiMediaRepository
{
    /// <summary>
    /// Returns a single Wiki page
    /// </summary>
    /// <param name="wikiPageId">ID/Name of the page</param>
    /// <returns>the page or null if it was not found</returns>
    Task<WikiPageDto?> GetWikiPageAsync(string wikiPageId);

    /// <summary>
    /// Runs a Cargo query against the wiki server. During enumeration, the query is executed in batches.
    /// </summary>
    /// <param name="tables">List of tables to include</param>
    /// <param name="fields">List of fields to include</param>
    /// <param name="where">Filter</param>
    /// <param name="orderBy">Sorting</param>
    /// <param name="pageSize">Size of each batch</param>
    /// <param name="cancellationToken">Token to cancel the query</param>
    /// <typeparam name="T">Type that can be converted from JSON matching the <paramref name="fields"/></typeparam>
    /// <returns></returns>
    IAsyncEnumerable<CargoQueryResponseItemDto<T>> RunCargoQueryAsync<T>(string[] tables, string[] fields,
        CargoQueryWhereClause[] where,
        string[] orderBy, int pageSize = 200, CancellationToken cancellationToken = default);
}