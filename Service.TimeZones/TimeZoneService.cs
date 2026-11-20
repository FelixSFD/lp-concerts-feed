namespace Service.TimeZones;

public class TimeZoneService(string tzApiBaseUrl, HttpClient httpClient) : ITimeZoneService
{
    public TimeZoneInfo GetTimeZoneByCoordinates(double lat, double lon)
    {
        throw new NotImplementedException();
    }
}