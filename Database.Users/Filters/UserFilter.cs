using Common.Database.Filter;
using Database.Users.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Users.Filters;

/// <summary>
/// Filter for users
/// </summary>
public class UserFilter : IQueryFilter<UserDo>
{
    /// <summary>
    /// Name of the user
    /// </summary>
    public string? Username { get; set; }
    
    /// <inheritdoc />
    public IQueryable<UserDo> ApplyTo(IQueryable<UserDo> query)
    {
        return query.Where(x => EF.Functions.Like(x.Username, $"%{Username}%"));
    }
}