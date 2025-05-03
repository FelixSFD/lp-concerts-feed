using System.Globalization;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using LPCalendar.DataStructure.Responses;
using Xunit;

namespace Lambda.GetTimeZone.Tests;

public class FunctionTest
{
    private readonly string _mockApiKey = "MYDUMMYKEY";
    
    public FunctionTest()
    {
    }


    private static ILambdaContext CreateLambdaContext()
    {
        return new TestLambdaContext();
    }


    [Theory]
    [InlineData("12.329482", "-45.234131", "America/New_York", "TestData/TimeZoneDb/new_york.json")]
    [InlineData("47.5229165", "10.2284546", "Europe/Berlin", "TestData/TimeZoneDb/gunzesried.json")]
    public async Task FunctionHandler(string lat, string lon, string expectedTzId, string mockApiResult)
    {
        // prepare the http mocks
        var httpResponseMessageMock = new HttpResponseMessageMockBuilder()
            .Where(httpRequestMessage => httpRequestMessage.Method == HttpMethod.Get &&
                                         httpRequestMessage.RequestUri != null &&
                                         httpRequestMessage.RequestUri
                                             .PathAndQuery
                                             .Equals($"/v2.1/get-time-zone?key={_mockApiKey}&by=position&lat={lat}&lng={lon}&format=json")
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
        
        // Make the function
        var func = new Function(httpClient, _mockApiKey);
        
        var proxyRequest = new APIGatewayProxyRequest()
        {
            HttpMethod = HttpMethod.Get.Method,
            Path = "/timeZone/byCoordinates",
            QueryStringParameters = new Dictionary<string, string>
            {
                ["lat"] = lat,
                ["lon"] = lon
            }
        };
        
        var response = await func.FunctionHandler(proxyRequest, CreateLambdaContext());
        
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

        var responseObj = JsonSerializer.Deserialize<GetTimeZoneByCoordinatesResponse>(response.Body);
        Assert.Equal(expectedTzId, responseObj?.TimeZoneId);
    }
}