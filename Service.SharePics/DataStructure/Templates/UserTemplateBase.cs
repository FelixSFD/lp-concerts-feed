namespace Service.SharePics.DataStructure.Templates;

/// <summary>
/// Base for templates that handle user-data
/// </summary>
public abstract class UserTemplateBase : SharePicTemplate
{
    /// <summary>
    /// Name of the user
    /// </summary>
    public required string Username { get; init; }
    
    /// <summary>
    /// ID of the user
    /// </summary>
    public required string UserId { get; init; }
}