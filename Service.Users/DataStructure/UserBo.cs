namespace Service.Users.DataStructure;

/// <summary>
/// Information about a user
/// </summary>
public class UserBo
{
    /// <summary>
    /// ID of the user
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Display name of the user
    /// </summary>
    public required string Username { get; set; }
}