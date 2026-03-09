namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception to throw when a requested mashup could not be created because the request was invalid
/// </summary>
public class InvalidMashupException(string message) : SetlistServiceException(message)
{
}