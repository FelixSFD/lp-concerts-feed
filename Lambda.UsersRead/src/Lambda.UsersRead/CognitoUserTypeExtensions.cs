using Amazon.CognitoIdentityProvider.Model;

namespace Lambda.UsersRead;

internal static class CognitoUserTypeExtensions
{
    internal static string? GetAttributeFromUser(this UserType user, string attributeName)
    {
        return user.Attributes
            .FirstOrDefault(at => at.Name == attributeName)?
            .Value? // read the actual value of the attribute
            .ToString();
    }
}