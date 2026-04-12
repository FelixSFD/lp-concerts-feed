using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Importer;

public interface IWikitextParser
{
    WikiSetlistEntry[] GetEntries(string setlistSource);

    /// <summary>
    /// Extract the part of the source that renders the setlist
    /// </summary>
    /// <param name="pageSource">Source of the whole wiki page</param>
    /// <returns>the setlist part or null if no setlist was found</returns>
    string? ExtractSetlistSource(string pageSource);
}