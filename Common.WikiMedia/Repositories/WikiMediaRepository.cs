using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Common.WikiMedia.DTOs;
using Microsoft.Extensions.Logging;

namespace Common.WikiMedia.Repositories;

/// <summary>
/// Repository to read data from a MediaWiki instance
/// </summary>
/// <param name="httpClient"></param>
/// <param name="restApiBaseUrl">Base URL for the REST-API</param>
/// <param name="restApiBaseUrl">Base URL for the Action-API</param>
public class WikiMediaRepository(HttpClient httpClient, string restApiBaseUrl, string actionApiBaseUrl, ILogger<WikiMediaRepository>? logger) : IWikiMediaRepository
{
    /// <summary>
    /// Helper to generate the API URLs for the REST API
    /// </summary>
    private readonly ApiUrlBuilder _restApiUrlBuilder = new(restApiBaseUrl);
    
    /// <summary>
    /// Helper to generate the API URLs for the Action API
    /// </summary>
    private readonly ApiUrlBuilder _actionApiUrlBuilder = new(actionApiBaseUrl);
    
    /// <inheritdoc/>
    public async Task<WikiPageDto?> GetWikiPageAsync(string wikiPageId)
    {
        var url = _restApiUrlBuilder.GetPageUrl(wikiPageId);
        var httpResponseMessage = await httpClient.GetAsync(url);
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            return null;
        }
        
        return await httpResponseMessage.Content.ReadFromJsonAsync<WikiPageDto>(WikiMediaJsonContext.Default.WikiPageDto);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<CargoQueryResponseItemDto<T>> RunCargoQueryAsync<T>(string[] tables, string[] fields, CargoQueryWhereClause[] where,
        string[] orderBy, int pageSize = 200, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger?.LogDebug("Starting cargo query: Tables: {tables}; Fields: {fields}; Where: {where}; Order by: {orderBy}; Page size: {pageSize}", tables, fields, where, orderBy, pageSize);
        
        var offset = 0;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger?.LogDebug("Cargo query was cancelled.");
                break;
            }
            
            var url = _actionApiUrlBuilder.GetCargoQueryUrl(tables, fields, where, orderBy, pageSize, offset);
            logger?.LogDebug("Get next page from URL: {url}", url);
            var queryResponse = await httpClient.GetFromJsonAsync<CargoQueryResponseDto<T>>(url, cancellationToken);
            if (queryResponse == null)
            {
                logger?.LogWarning("Cargo query returned null. Maybe JSON parsing failed?");
                break;
            }

            foreach (var item in queryResponse.Results)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger?.LogDebug("Cargo query was cancelled.");
                    break;
                }
                
                yield return item;
            }
            
            if (queryResponse.Results.Length != pageSize)
            {
                logger?.LogDebug("Cargo query returned less results than requested. This is the last page.");
                break;
            }
            
            offset += pageSize;
            logger?.LogDebug("Next page offset: {offset}", offset);
        }
        
        logger?.LogDebug("Finished cargo query and returned all results from all pages.");
    }
}