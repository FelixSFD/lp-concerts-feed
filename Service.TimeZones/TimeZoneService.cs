using System.Net.Http.Json;
using Service.TimeZones.DataStructure;
using TimeZoneNotFoundException = Service.TimeZones.Exceptions.TimeZoneNotFoundException;

namespace Service.TimeZones;

public class TimeZoneService(string tzApiBaseUrl, string apiKey, HttpClient httpClient) : ITimeZoneService
{
    /// <summary>
    /// Retrieves the time zone information based on the provided latitude and longitude coordinates.
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <returns></returns>
    /// <exception cref="TimeZoneNotFoundException">Thrown when the time zone could not be found for the provided coordinates.</exception>
    public async Task<TimeZoneInfo> GetTimeZoneByCoordinates(double lat, double lon)
    {
        var uri = new Uri($"{tzApiBaseUrl}/get-time-zone?key={apiKey}&by=position&lat={lat}&lng={lon}&format=json");
        var httpResponseMessage = await httpClient.GetAsync(uri);
        var timeZoneDbResponse = await httpResponseMessage.Content.ReadFromJsonAsync<TimeZoneDbResponseDto>() ?? throw new TimeZoneNotFoundException(lat, lon);
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneDbResponse.ZoneName);
    }
}