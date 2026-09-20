using Common.Database.Filter;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Filters;

/// <summary>
/// Filter for tours
/// </summary>
public class TourFilter : IQueryFilter<TourDo>
{
    /// <summary>
    /// Search by name
    /// </summary>
    public string? Name { get; set; }
    
    /// <inheritdoc />
    public IQueryable<TourDo> ApplyTo(IQueryable<TourDo> query)
    {
        if (!string.IsNullOrEmpty(Name))
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{Name}%"));
        
        return query;
    }
}