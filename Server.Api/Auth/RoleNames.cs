namespace Server.Api.Auth;

/// <summary>
/// Names of the roles that can be used for authorization
/// </summary>
/// <seealso cref="AuthorizeRolesAttribute"/>
public static class RoleNames
{
    /// <summary>
    /// Role of administrators
    /// </summary>
    public const string Admin = "Admin";
    
    /// <summary>
    /// People in this group can add concerts
    /// </summary>
    public const string AddConcerts = "AddConcerts";
    
    /// <summary>
    /// People in this group can DELETE concerts
    /// </summary>
    public const string DeleteConcerts = "DeleteConcerts";
    
    /// <summary>
    /// People in this group can update concerts
    /// </summary>
    public const string UpdateConcerts = "UpdateConcerts";
    
    /// <summary>
    /// People in this group can delete songs
    /// </summary>
    public const string DeleteSongs = "DeleteSongs";
    
    /// <summary>
    /// People in this group can manage setlists
    /// </summary>
    public const string ManageSetlists = "ManageSetlists";
    
    /// <summary>
    /// People in this group can manage users
    /// </summary>
    public const string ManageUsers = "ManageUsers";
}