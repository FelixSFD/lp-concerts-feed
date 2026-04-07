using Amazon.Lambda.Core;
using Common.WikiMedia.Repositories;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists.Import;
using Service.Setlists.Importer;
using Service.Setlists.Importer.DataStructure;
using Service.Setlists.Importer.Exceptions;

namespace Service.Setlists;

/// <summary>
/// Service to import data from Linkinpedia
/// </summary>
public class LinkinpediaImportService(IWikiMediaRepository wikiMediaRepository, IWikitextParser wikitextParser, ISongRepository songRepository, ILambdaLogger logger)
{
    public const string LinkinpediaRestApiBaseUrl = "https://linkinpedia.com/w/rest.php/v1";
    
    public async Task<ImportSetlistPreviewDto> GetImportPlanForSetlistFromPageAsync(string wikiPageId)
    {
        logger.LogDebug("Importing from Linkinpedia Page: {page}", wikiPageId);
        var wikiPage = await wikiMediaRepository.GetWikiPageAsync(wikiPageId) ?? throw new InvalidWikiContentException($"Could not find WikiPage: {wikiPageId}");
        logger.LogDebug("Downloaded page '{title}', which was last updated: {lastUpdated}", wikiPage.Title, wikiPage.Latest?.Timestamp);
        
        // read the content for the page
        var wikiContent = wikiPage.Source ?? throw new InvalidWikiContentException("Page source is empty");
        var setlistWikitext = wikitextParser.ExtractSetlistSource(wikiContent) ?? throw new InvalidWikiContentException("Page source does not contain a setlist", wikiContent);

        var parsedEntries = wikitextParser.GetEntries(setlistWikitext);
        logger.LogDebug("Found {count} entries in setlist.", parsedEntries.Length);

        var songEntries = parsedEntries.Where(e => e.GetType() == typeof(SongWikiSetlistEntry));

        var mappedEntries = await Task.WhenAll(songEntries.Select(GetPreviewEntryAsync));

        var setlistPreview = new ImportSetlistPreviewDto
        {
            ConcertId = "1234",
            Entries = mappedEntries.ToList()
        };
        
        return setlistPreview;
    }

    private async Task<ImportSetlistEntryPreviewDto> GetPreviewEntryAsync(WikiSetlistEntry wikiSetlistEntry)
    {
        ImportSetlistEntryPreviewDto preview = new()
        {
            Title = wikiSetlistEntry.Name ?? "Unknown",
            ExtraNotes = wikiSetlistEntry.Note
        };

        if (wikiSetlistEntry.GetType() == typeof(SongWikiSetlistEntry))
        {
            logger.LogDebug("Entry is a song. Check if the song '{title}' exists in our database...", wikiSetlistEntry.Name);
            var existingSongs = await songRepository.GetSongsByTitle(preview.Title).ToArrayAsync();
            try
            {
                var uniqueSong = existingSongs.SingleOrDefault();
                if (uniqueSong != null)
                {
                    logger.LogDebug("Song '{title}' has an exact match with ID {songId}.", wikiSetlistEntry.Name, uniqueSong.Id);
                    preview.FoundSongId = uniqueSong.Id;
                }
                else
                {
                    logger.LogWarning("Song '{title}' could not be found.", wikiSetlistEntry.Name);
                }
            }
            catch (InvalidOperationException exception)
            {
                logger.LogInformation("Song '{title}' has {count} matches.", wikiSetlistEntry.Name, existingSongs.Length);
                preview.FoundSongIds = existingSongs.Select(s => s.Id).ToArray();
            }

            preview.SongNumber = ((SongWikiSetlistEntry)wikiSetlistEntry).SongNumber;
        }
        
        return preview;
    }
}