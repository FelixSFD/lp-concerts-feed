using System.Text.Json.Serialization;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.SQS.Model;

namespace Lambda.UsersRead;

[JsonSerializable(typeof(SendMessageRequest))]
[JsonSerializable(typeof(UserType))]
[JsonSerializable(typeof(List<UserType>))]
public partial class LocalJsonContext: JsonSerializerContext
{
    
}