namespace Service.Setlists.Importer.Exceptions;

/// <summary>
/// Exception thrown by the <see cref="LinkinpediaImportService"/>
/// </summary>
public class LinkinpediaImportServiceException(string message) : Exception($"Import from Linkinpedia failed: {message}")
{
    
}