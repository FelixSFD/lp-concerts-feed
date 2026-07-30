namespace Service.Tours.Exceptions;

/// <summary>
/// Base class for errors where an object was not found
/// </summary>
/// <param name="objectName"></param>
/// <param name="keyValues"></param>
public class NotFoundExceptionBase(string objectName, params string[] keyValues) : Exception($"{objectName} with key {string.Join(", ", keyValues.Select(k => $"'{k}'"))} not found!")
{
}