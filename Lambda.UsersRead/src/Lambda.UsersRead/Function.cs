using System.Net;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Auth;
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
        if (!request.IsMemberOf("Admin"))
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, POST, PUT");
        }
        
        string? idParameter = null;
        var hasIdParam = request.PathParameters != null && request.PathParameters.TryGetValue("id", out idParameter);
        if (request.HttpMethod == "GET")
        {
            if (hasIdParam)
            {
                context.Logger.LogInformation("Requested ID: {id}", idParameter);
                return await ReturnSingleUser(idParameter!, context);
            }
        
            context.Logger.LogDebug("Requested list of users");
            return await ReturnAllUsers(context);
        } else if (request.HttpMethod == "PUT" && hasIdParam)
        {
            var sentUser = JsonSerializer.Deserialize<User>(request.Body);
            if (sentUser == null)
            {
                return new APIGatewayProxyResponse()
                {
                    // TODO: Error message?
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" },
                        { "Access-Control-Allow-Origin", "*" },
                        { "Access-Control-Allow-Methods", "OPTIONS, POST, PUT" }
                    }
                };
            }
            
            // use ID from parameter
            sentUser.Id = idParameter!;
            
            return await UpdateUser(sentUser, context);
        }
        else
        {
            return new APIGatewayProxyResponse()
            {
                // TODO: Error message?
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
    }


    private async Task<APIGatewayProxyResponse> ReturnAllUsers(ILambdaContext context)
    {
        var userTypes = await ListUsersAsync();
        
        context.Logger.LogDebug($"Found {userTypes.Count} users.\n\n{JsonSerializer.Serialize(userTypes)}");
        
        var users = userTypes.Select(ut => ut.Attributes.ToUser());
        
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
    
    
    private async Task<APIGatewayProxyResponse> ReturnSingleUser(string userId, ILambdaContext context)
    {
        context.Logger.LogDebug("Returning single user");
        var user = await GetUserByIdAsync(userId);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(user),
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
    /// <returns>A list of users.</returns>
    private async Task<List<UserType>> ListUsersAsync()
    {
        var request = new ListUsersRequest
        {
            UserPoolId = _userPoolId
        };

        var users = new List<UserType>();

        var usersPaginator = _cognitoService.Paginators.ListUsers(request);
        await foreach (var response in usersPaginator.Responses)
        {
            users.AddRange(response.Users);
        }

        return users;
    }


    /// <summary>
    /// Get a user for the Amazon Cognito user pool.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns>A user.</returns>
    private async Task<User> GetUserByIdAsync(string userId)
    {
        var request = new AdminGetUserRequest
        {
            UserPoolId = _userPoolId,
            Username = userId
        };
        
        var response = await _cognitoService.AdminGetUserAsync(request) ?? throw new KeyNotFoundException("User was not found");
        var user = response.UserAttributes.ToUser();

        var groupsRequest = new AdminListGroupsForUserRequest
        {
            UserPoolId = _userPoolId,
            Username = userId
        };
        
        // add groups
        var groupsResponse = await _cognitoService.AdminListGroupsForUserAsync(groupsRequest) ?? throw new KeyNotFoundException("User was not found; Could not fetch groups.");
        user.UserGroups = groupsResponse.Groups?.Select(gt => gt.ToUserGroup()).ToList() ?? [];

        return user;
    }


    private async Task<APIGatewayProxyResponse> UpdateUser(User user, ILambdaContext context)
    {
        var request = new AdminUpdateUserAttributesRequest
        {
            UserPoolId = _userPoolId,
            Username = user.Id,
            UserAttributes =
            [
                new AttributeType
                {
                    Name = "email",
                    Value = user.Email
                },
                new AttributeType
                {
                    Name = "custom:display_name",
                    Value = user.Username
                }
            ]
        };
        
        var response = await _cognitoService.AdminUpdateUserAttributesAsync(request);
        return new APIGatewayProxyResponse
        {
            StatusCode = response.HttpStatusCode == HttpStatusCode.OK ? (int)HttpStatusCode.NoContent : (int)response.HttpStatusCode,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, PUT, POST" }
            }
        };
    }
}