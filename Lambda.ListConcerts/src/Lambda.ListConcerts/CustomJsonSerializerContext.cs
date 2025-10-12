using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using LPCalendar.DataStructure;

namespace Lambda.ListConcerts;

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Concert))]
public partial class CustomJsonSerializerContext : JsonSerializerContext
{
}