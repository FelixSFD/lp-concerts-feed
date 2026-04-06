using Amazon.Lambda.Core;
using Common.WikiMedia.Repositories;
using Service.Setlists.Importer;
using Service.Setlists.Importer.Exceptions;

namespace Service.Setlists;

/// <summary>
/// Service to import data from Linkinpedia
/// </summary>
public class LinkinpediaImportService(WikiMediaRepository wikiMediaRepository, WikitextParser wikitextParser, ILambdaLogger logger)
{
    const string LinkinpediaRestApiBaseUrl = "https://linkinpedia.com/w/rest.php/v1";
    
    public async Task ImportSetlistFromPage(string wikiPageId)
    {
        var wikiPage = await wikiMediaRepository.GetWikiPageAsync(wikiPageId);
        if (wikiPage == null)
        {
            logger.LogError($"Wiki Page {wikiPageId} not found");
            return;
        }
        
        // read the content for the page
        var wikiContent = wikiPage.Source ?? throw new InvalidWikiContentException("Page source is empty");
        var setlistWikitext = wikitextParser.ExtractSetlistSource(wikiContent) ?? throw new InvalidWikiContentException("Page source does not contain a setlist", wikiContent);

        var parsedEntries = wikitextParser.GetEntries(setlistWikitext);
        logger.LogDebug("Found {count} entries in setlist.", parsedEntries.Length);
    }
}