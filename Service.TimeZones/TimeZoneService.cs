using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Service.TimeZones.DataStructure;
using TimeZoneNotFoundException = Service.TimeZones.Exceptions.TimeZoneNotFoundException;

namespace Service.TimeZones;

public class TimeZoneService(string tzApiBaseUrl, string apiKey, HttpClient httpClient, ILogger<TimeZoneService> logger) : ITimeZoneService
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
        try
        {
            var uri = new Uri($"{tzApiBaseUrl}/get-time-zone?key={apiKey}&by=position&lat={lat.ToString(CultureInfo.InvariantCulture)}&lng={lon.ToString(CultureInfo.InvariantCulture)}&format=json");
            var httpResponseMessage = await httpClient.GetAsync(uri);
            httpResponseMessage.EnsureSuccessStatusCode();
            var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
            Log.ReceivedResponseFromTimeZoneDbApi(logger, lat, lon, responseString);
            var timeZoneDbResponse = await httpResponseMessage.Content.ReadFromJsonAsync<TimeZoneDbResponseDto>() ?? throw new TimeZoneNotFoundException(lat, lon);
            
            if (timeZoneDbResponse.Status == "FAILED")
            {
                Log.TimeZoneApiCallFailed(logger, lat, lon, timeZoneDbResponse.Message ?? "No message provided");
                throw new TimeZoneNotFoundException($"Time zone lookup failed for coordinates: lat={lat}, lon={lon}. Message: {timeZoneDbResponse.Message}");
            }
            
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneDbResponse.ZoneName);
        } catch (Exception ex)
        {
            throw new TimeZoneNotFoundException($"Could not retrieve time zone for coordinates: lat={lat}, lon={lon}", ex);
        }
    }
}