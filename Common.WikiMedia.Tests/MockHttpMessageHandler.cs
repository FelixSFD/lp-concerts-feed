using System.Net;

namespace Common.WikiMedia.Tests;

public class MockHttpMessageHandler(string response, HttpStatusCode statusCode) : HttpMessageHandler
{
    public string? Input { get; private set; }
    public int NumberOfCalls { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        NumberOfCalls++;
        if (request.Content != null) // Could be a GET-request without a body
        {
            Input = await request.Content.ReadAsStringAsync(cancellationToken);
        }
        return new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(response)
        };
    }
}