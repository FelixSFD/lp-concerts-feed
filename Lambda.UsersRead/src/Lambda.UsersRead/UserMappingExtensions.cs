using Amazon.CognitoIdentityProvider.Model;
using LPCalendar.DataStructure;

namespace Lambda.UsersRead;

internal static class UserMappingExtensions
{
    internal static string? GetAttributeFromUser(this UserType user, string attributeName)
    {
        return user.Attributes
            .GetAttribute(attributeName);
    }
    
    
    /// <summary>
    /// Returns a given attribute from the list as string
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="attributeName"><see cref="AttributeType.Name"/></param>
    /// <returns></returns>
    private static string? GetAttribute(this List<AttributeType> attributes, string attributeName)
    {
        return attributes
            .FirstOrDefault(at => at.Name == attributeName)?
            .Value? // read the actual value of the attribute
            .ToString();
    }
    
    
    /// <summary>
    /// Creates a <see cref="User"/> object based on the list of attributes
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    internal static User ToUser(this List<AttributeType> attributes)
    {
        return new User
        {
            Id = attributes.GetAttribute("sub")!,
            Username = attributes.GetAttribute("custom:display_name") ?? "No name",
            Email = attributes.GetAttribute("email")!,
            EmailVerified = attributes.GetAttribute("email_verified") == "true"
        };
    }
}