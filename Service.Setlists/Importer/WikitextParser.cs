using System.Text.RegularExpressions;
using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Importer;

/// <summary>
/// Class to parse the source wikitext from Linkinpedia
/// </summary>
public partial class WikitextParser
{
    [GeneratedRegex(@"{{Setlist[\s\S-[}]]*}}", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ExtractSetlistFromSourceRegex { get; }
    
    public async Task<WikiSetlistEntry[]> GetEntriesAsync(string setlistSource)
    {
        return [];
    }

    /// <summary>
    /// Extract the part of the source that renders the setlist
    /// </summary>
    /// <param name="pageSource">Source of the whole wiki page</param>
    /// <returns>the setlist part or null if no setlist was found</returns>
    public async Task<string?> ExtractSetlistSource(string pageSource)
    {
        var match = ExtractSetlistFromSourceRegex.Match(pageSource);
        return match is not { Success: true } ? null : match.Value;
    }
}