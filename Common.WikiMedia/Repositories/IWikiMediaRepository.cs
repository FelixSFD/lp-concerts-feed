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
}