using Common.Database.Filter;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Filters;

/// <summary>
/// Filter for countries
/// </summary>
public class CountryFilter: IQueryFilter<CountryDo>
{
    /// <summary>
    /// Filter by ISO code
    /// </summary>
    public string? IsoCode { get; set; }
    
    /// <summary>
    /// Filter by name (English)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by native name
    /// </summary>
    public string? NativeName { get; set; }
    
    /// <inheritdoc />
    public IQueryable<CountryDo> ApplyTo(IQueryable<CountryDo> query)
    {
        if (!string.IsNullOrEmpty(IsoCode))
            query = query.Where(x => x.IsoCode == IsoCode);

        if (!string.IsNullOrEmpty(Name))
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{Name}%"));

        if (!string.IsNullOrEmpty(NativeName))
            query = query.Where(x => EF.Functions.Like(x.NativeName, $"%{NativeName}%"));

        return query;
    }
}