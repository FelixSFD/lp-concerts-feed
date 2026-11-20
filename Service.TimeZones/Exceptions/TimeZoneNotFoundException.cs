namespace Service.TimeZones.Exceptions;

/// <summary>
/// Exception thrown when a time zone could not be found
/// </summary>
public class TimeZoneNotFoundException : Exception
{
    public TimeZoneNotFoundException(double lat, double lon) : base($"Time zone not found for coordinates: lat={lat}, lon={lon}")
    {
    }
    
    public TimeZoneNotFoundException(string message) : base(message)
    {
    }
    
    public TimeZoneNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}