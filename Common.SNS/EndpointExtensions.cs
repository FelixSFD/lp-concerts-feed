using Amazon.SimpleNotificationService.Model;

namespace Lambda.Push.Sender;

/// <summary>
/// Extensions for the AWS SNS Endpoint model
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Returns true if the endpoint is enabled. The data is read from the attributes dictionary
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public static bool IsEnabled(this Endpoint endpoint)
    {
        //https://docs.aws.amazon.com/sns/latest/api/API_GetEndpointAttributes.html
        var enabled = endpoint.Attributes.GetValueOrDefault("Enabled", "false");
        _ = bool.TryParse(enabled, out var parsedEnabled);
        return parsedEnabled;
    }
    
    
    /// <summary>
    /// Returns true if the endpoint is enabled. The data is read from the attributes dictionary
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static bool IsEnabled(this GetEndpointAttributesResponse response)
    {
        //https://docs.aws.amazon.com/sns/latest/api/API_GetEndpointAttributes.html
        var enabled = response.Attributes.GetValueOrDefault("Enabled", "false");
        _ = bool.TryParse(enabled, out var parsedEnabled);
        return parsedEnabled;
    }
}