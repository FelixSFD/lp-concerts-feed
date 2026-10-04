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
    
    /// <summary>
    /// ISO code of the country where this user is from. Can be null if the user has not specified their country
    /// </summary>
    public string? OriginCountryCode { get; set; }

    /// <summary>
    /// Date and time when the user was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Date and time when the user was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}