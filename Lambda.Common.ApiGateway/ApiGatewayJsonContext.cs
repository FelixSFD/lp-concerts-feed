using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace Lambda.Common.ApiGateway;

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class ApiGatewayJsonContext : JsonSerializerContext
{
}