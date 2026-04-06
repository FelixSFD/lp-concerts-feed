using System.Net.Http.Json;
using Common.WikiMedia.DTOs;

namespace Common.WikiMedia.Repositories;

/// <summary>
/// Repository to read data from a MediaWiki instance
/// </summary>
/// <param name="httpClient"></param>
/// <param name="baseUrl"></param>
public class WikiMediaRepository(HttpClient httpClient, string baseUrl) : IWikiMediaRepository
{
    /// <summary>
    /// Helper to generate the API URLs
    /// </summary>
    private readonly ApiUrlBuilder _apiUrlBuilder = new(baseUrl);
    
    /// <inheritdoc/>
    public async Task<WikiPageDto?> GetWikiPageAsync(string wikiPageId)
    {
        var url = _apiUrlBuilder.GetPageUrl(wikiPageId);
        var httpResponseMessage = await httpClient.GetAsync(url);
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return null;
        }
        
        return await httpResponseMessage.Content.ReadFromJsonAsync<WikiPageDto>(WikiMediaJsonContext.Default.WikiPageDto);
    }
}