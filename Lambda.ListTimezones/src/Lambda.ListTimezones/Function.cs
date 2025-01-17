using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ListTimezones;

/// <summary>
/// Function to list all possible timezones.
/// This can be used to help the users choose a timezone
/// </summary>
public class Function
{
    public APIGatewayProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var response = new APIGatewayProxyResponse()
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };

        var timeZoneEntries = TimeZoneInfo
            .GetSystemTimeZones()
            .Select(GetTimeZoneApiResponse)
            .ToArray();

        var json = JsonSerializer.Serialize(timeZoneEntries);
        response.Body = json;
        response.StatusCode = (int)HttpStatusCode.OK;

        return response;
    }


    private static TimeZoneEntry GetTimeZoneApiResponse(TimeZoneInfo info)
    {
        return new TimeZoneEntry
        {
            Id = info.Id,
            DisplayName = info.DisplayName
        };
    }
}