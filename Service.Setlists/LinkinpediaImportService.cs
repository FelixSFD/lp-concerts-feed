using Amazon.Lambda.Core;

namespace Service.Setlists;

/// <summary>
/// Service to import data from Linkinpedia
/// </summary>
public class LinkinpediaImportService(HttpClient httpClient, ILambdaLogger logger)
{
    const string LinkinpediaRestApiBaseUrl = "https://linkinpedia.com/w/rest.php/v1";
    
    public async Task ImportSetlistFromPage(string wikiPageId)
    {
        var url = new Uri(LinkinpediaRestApiBaseUrl + "/page/" + wikiPageId);
        logger.LogDebug("Download the wiki page at: {url}", url);
        await httpClient.GetAsync(url);
    }
}