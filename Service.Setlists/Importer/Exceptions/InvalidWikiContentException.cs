namespace Service.Setlists.Importer.Exceptions;

public class InvalidWikiContentException(string message, string? content = null) : LinkinpediaImportServiceException($"Content of wiki entry is not valid: {message}")
{
    public string? Content { get; set; } = content;
}