using System.Net;
using NSubstitute;

namespace Service.TimeZones.Tests;

public class TimeZoneServiceTest
{
    private readonly HttpClient _httpClient;
    private readonly TimeZoneService _timeZoneService;

    public TimeZoneServiceTest()
    {
        _httpClient = Substitute.For<HttpClient>();
        _timeZoneService = new TimeZoneService("https://example.com", _httpClient);
    }

    [Theory]
    [InlineData(40.7128, -74.0060, "America/New_York")]
    [InlineData(51.5074, -0.1278, "Europe/London")]
    public void GetTimeZoneByCoordinates(double latitude, double longitude, string mockTimeZoneId)
    {
        // Arrange
        var expectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(mockTimeZoneId);
        _httpClient.GetAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(mockTimeZoneId)
        });

        // Act
        var result = _timeZoneService.GetTimeZoneByCoordinates(latitude, longitude);

        // Assert
        Assert.Equal(expectedTimeZone, result);
        _httpClient.Received(1).GetAsync(Arg.Any<string>());
    }
}