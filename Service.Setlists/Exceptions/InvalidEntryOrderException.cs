namespace Service.Setlists.Exceptions;

public class InvalidEntryOrderException(string message) : SetlistServiceException($"Failed to reorder entries: {message}")
{
}