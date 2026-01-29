using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace Common.TestUtils;

public class ApiRequestBuilder
{
    private readonly List<string> _path = [];
    private string? _body;
    private HttpMethod? _httpMethod;
    private Dictionary<string, string>? _pathParams;
    private Dictionary<string, string>? _headers;

    public ApiRequestBuilder WithPathParameter(string name, string value)
    {
        _pathParams ??= new(StringComparer.OrdinalIgnoreCase);
        _pathParams.Add(name, value);
        _path.Add(value);
        return this;
    }
    
    public ApiRequestBuilder WithPath(params string[] parts)
    {
        _path.AddRange(parts);
        return this;
    }

    public ApiRequestBuilder WithHttpMethod(string httpMethod) =>
        WithHttpMethod(new HttpMethod(httpMethod));

    public ApiRequestBuilder WithHttpMethod(HttpMethod httpMethod)
    {
        _httpMethod = httpMethod;
        return this;
    }

    public ApiRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public ApiRequestBuilder WithBody<T>(T body, JsonSerializerOptions? jsonOptions = default)
    {
        _body = JsonSerializer.Serialize(body, jsonOptions);
        return this;
    }

    public ApiRequestBuilder WithHeader(string name, string value)
    {
        _headers ??= new(StringComparer.OrdinalIgnoreCase);
        _headers[name] = value;
        return this;
    }

    public APIGatewayProxyRequest Build()
    {
        var path = "/" + string.Join('/', _path);
        return new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
            {
                DomainName = "localhost",
                Path = path,
                HttpMethod = (_httpMethod ?? HttpMethod.Get).Method,
            },
            PathParameters = _pathParams,
            Path = path,
            Body = _body,
            Headers = _headers,
        };
    }
}