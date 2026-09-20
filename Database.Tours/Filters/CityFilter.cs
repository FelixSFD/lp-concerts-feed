using Common.Database.Filter;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Filters;

/// <summary>
/// Filter for cities
/// </summary>
public class CityFilter : IQueryFilter<CityDo>
{
    /// <summary>
    /// Search by exact ISO code of the country
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
    public IQueryable<CityDo> ApplyTo(IQueryable<CityDo> query)
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