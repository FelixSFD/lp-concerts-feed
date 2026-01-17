using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Common.SNS;

public static class AmazonSimpleNotificationServiceExtensions
{
    /// <summary>
    /// Returns all endpoints as async enumerable. Might run several requests to the SNS API, depending on how many were found.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="platformApplicationArn"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<Endpoint> FindAllEndpointsByPlatformApplicationAsync(this IAmazonSimpleNotificationService service, string platformApplicationArn)
    {
        var request = new ListEndpointsByPlatformApplicationRequest
        {
            PlatformApplicationArn = platformApplicationArn
        };
        
        ListEndpointsByPlatformApplicationResponse response;
        do
        {
            response = await service.ListEndpointsByPlatformApplicationAsync(request);
            foreach (var endpoint in response.Endpoints)
            {
                yield return endpoint;
            }
            
            request.NextToken = response.NextToken;
        } while (response.Endpoints.Count > 0 && !string.IsNullOrEmpty(response.NextToken));
    }
}