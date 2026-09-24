using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Common.Server.ExceptionHandling;

public sealed class TextPlainProblemDetailsWriter : IProblemDetailsWriter
{
    private static readonly MediaTypeHeaderValue _jsonMediaType = new("text/plain");
    
    public bool CanWrite(ProblemDetailsContext context)
    {
        var acceptHeader = context.HttpContext.Request.GetTypedHeaders().Accept;

        // Based on https://www.rfc-editor.org/rfc/rfc7231#section-5.3.2 a request
        // without the Accept header implies that the user agent
        // will accept any media type in response
        if (acceptHeader.Count == 0)
        {
            return true;
        }
        for (var i = 0; i < acceptHeader.Count; i++)
        {
            var acceptHeaderValue = acceptHeader[i];
            // Check to see if the Accepted header values support `application/json` or `application/problem+json`
            // with  support for argument parameters. Support handling `*/*` and `application/*` as Accepts header values.
            // Application/json is a subset of */* but */* is not a subset of application/json
            if (acceptHeaderValue.IsSubsetOf(_jsonMediaType) || _jsonMediaType.IsSubsetOf(acceptHeaderValue))
            {
                return true;
            }
        }
        
        return false;
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var response = context.HttpContext.Response;

        await response.WriteAsJsonAsync(
            context.ProblemDetails,
            cancellationToken: context.HttpContext.RequestAborted);
    }
}
