using Common.Database.Filter;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Filters;

/// <summary>
/// Filter for states
/// </summary>
public class StateFilter : IQueryFilter<StateDo>
{
    /// <summary>
    /// Search by exact ISO code of the country the state is in
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Search by name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Search by native name
    /// </summary>
    public string? NativeName { get; set; }
    
    /// <inheritdoc />
    public IQueryable<StateDo> ApplyTo(IQueryable<StateDo> query)
    {
        if (!string.IsNullOrEmpty(CountryCode))
            query = query.Where(x => x.CountryCode == CountryCode);
        
        if (!string.IsNullOrEmpty(Name))
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{Name}%"));

        if (!string.IsNullOrEmpty(NativeName))
            query = query.Where(x => EF.Functions.Like(x.NativeName, $"%{NativeName}%"));

        return query;
    }
}