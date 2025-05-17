using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.UsersRead;

public class Function
{
    private readonly IAmazonCognitoIdentityProvider _cognitoService = new AmazonCognitoIdentityProviderClient();
    private readonly string _userPoolId = Environment.GetEnvironmentVariable("USER_POOL_ID") ?? throw new ArgumentNullException(nameof(_userPoolId));


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var userTypes = await ListUsersAsync(_userPoolId);
        
        context.Logger.LogDebug($"Found {userTypes.Count} users.\n\n{JsonSerializer.Serialize(userTypes)}");
        
        var users = userTypes.Select(ut => new User
        {
            Id = ut.GetAttributeFromUser("sub")!,
            Username = ut.GetAttributeFromUser("custom:display_name") ?? "No name",
            Email = ut.GetAttributeFromUser("email")!
        });
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(users),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }
    
    
    /// <summary>
    /// Get a list of users for the Amazon Cognito user pool.
    /// </summary>
    /// <param name="userPoolId">The user pool ID.</param>
    /// <returns>A list of users.</returns>
    private async Task<List<UserType>> ListUsersAsync(string userPoolId)
    {
        var request = new ListUsersRequest
        {
            UserPoolId = userPoolId
        };

        var users = new List<UserType>();

        var usersPaginator = _cognitoService.Paginators.ListUsers(request);
        await foreach (var response in usersPaginator.Responses)
        {
            users.AddRange(response.Users);
        }

        return users;
    }


    
}