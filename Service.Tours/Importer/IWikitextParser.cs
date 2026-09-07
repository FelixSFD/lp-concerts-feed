using Service.Tours.Importer.DataStructure;

namespace Service.Tours.Importer;

public interface IWikitextParser
{
    /// <summary>
    /// Parses the tourdate source and extracts the concert/tourdate information
    /// </summary>
    /// <param name="tourdateSource">Wikitext source of the Tourdate template or the whole wiki page</param>
    /// <returns>Parsed tourdate information or null if no valid tourdate was found</returns>
    WikiTourdateEntry? GetTourdateInformation(string tourdateSource);

    /// <summary>
    /// Extract the part of the source that renders the tourdate
    /// </summary>
    /// <param name="pageSource">Source of the whole wiki page</param>
    /// <returns>the tourdate part or null if no tourdate was found</returns>
    string? ExtractTourdateSource(string pageSource);
}
