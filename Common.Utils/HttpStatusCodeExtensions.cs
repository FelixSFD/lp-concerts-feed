using System.Net;

namespace Common.Utils;

public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Checks if the request was successful, meaning the status is 2xx or 3xx
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns>true if the status indicates success</returns>
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
    {
        return statusCode is >= HttpStatusCode.OK and <= HttpStatusCode.BadRequest;
    }
}