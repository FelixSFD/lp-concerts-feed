namespace Service.TimeZones;

/// <summary>
/// Service interface for retrieving time zone information
/// </summary>
public interface ITimeZoneService
{
    /// <summary>
    /// Retrieves the time zone information based on the provided latitude and longitude coordinates.
    /// </summary>
    /// <param name="lat">The latitude of the location.</param>
    /// <param name="lon">The longitude of the location.</param>
    /// <returns>The <see cref="TimeZoneInfo"/> object representing the time zone of the specified location.</returns>
    TimeZoneInfo GetTimeZoneByCoordinates(double lat, double lon);
}