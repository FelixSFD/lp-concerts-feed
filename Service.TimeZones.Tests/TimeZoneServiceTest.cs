using System.Net;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;

namespace Service.TimeZones.Tests;

/// <summary>Tests coordinate-based time zone lookups using TimeZoneDB responses.</summary>
public class TimeZoneServiceTest
{
    private const string DummyApiKey = "dummy_api_key";
    
    /// <summary>Checks that coordinates are sent to the API and the returned zone is resolved.</summary>
    [Theory]
    [InlineData(40.7128, -74.0060, "America/New_York", "TestData/TimeZoneDb/new_york.json")]
    [InlineData(47.5229165, 10.2284546, "Europe/Berlin", "TestData/TimeZoneDb/gunzesried.json")]
    public async Task GetTimeZoneByCoordinates(double latitude, double longitude, string expectedTimeZoneId, string mockApiResult)
    {
        // Arrange
        var httpResponseMessageMock = new HttpResponseMessageMockBuilder()
            .Where(httpRequestMessage => httpRequestMessage.Method == HttpMethod.Get &&
                                         httpRequestMessage.RequestUri != null &&
                                         httpRequestMessage.RequestUri
                                             .PathAndQuery
                                             .Equals($"/v2.1/get-time-zone?key={DummyApiKey}&by=position&lat={latitude}&lng={longitude}&format=json")
            )
            .RespondWith(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText(mockApiResult))
            })
            .Build();
        
        // add the mocks to the http handler
        var handler = new TestHttpMessageHandler();
        handler.MockHttpResponse(httpResponseMessageMock);

        // instantiate the http client with the test handler
        var httpClient = new HttpClient(handler);
        
        var service = new TimeZoneService("https://example.com/v2.1", DummyApiKey, httpClient);

        // Act
        var result = await service.GetTimeZoneByCoordinates(latitude, longitude);

        // Assert
        Assert.Equal(TimeZoneInfo.FindSystemTimeZoneById(expectedTimeZoneId), result);
    }
}