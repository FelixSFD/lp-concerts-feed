using Amazon.Lambda.Core;
using Common.WikiMedia.Repositories;

namespace Service.Setlists;

/// <summary>
/// Service to import data from Linkinpedia
/// </summary>
public class LinkinpediaImportService(WikiMediaRepository wikiMediaRepository, ILambdaLogger logger)
{
    const string LinkinpediaRestApiBaseUrl = "https://linkinpedia.com/w/rest.php/v1";
    
    public async Task ImportSetlistFromPage(string wikiPageId)
    {
        var wikiPage = await wikiMediaRepository.GetWikiPageAsync(wikiPageId);
        if (wikiPage == null)
        {
            return;
        }
        
        // read the content for the page
        var wikiContent = wikiPage.Source;
    }
}