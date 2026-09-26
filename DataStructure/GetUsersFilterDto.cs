using LPCalendar.DataStructure.Requests;

namespace LPCalendar.DataStructure;

/// <summary>
/// Filter a list of users
/// </summary>
public class GetUsersFilterDto : BaseFilterQuery
{
    /// <summary>
    /// Username to filter by
    /// </summary>
    public string? Username { get; set; }
    
    public override string[] OrderBy { get; set; } = ["username"];
}