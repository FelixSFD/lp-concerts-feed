namespace Service.Setlists.Exceptions;

/// <summary>
/// Exception thrown in the <see cref="SetlistService"/>
/// </summary>
public class SetlistServiceException(string message = "Service threw an exception!") : Exception(message)
{
}