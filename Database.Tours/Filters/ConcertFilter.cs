using Common.Database.Filter;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Filters;

/// <summary>
/// Filter for concerts
/// </summary>
public class ConcertFilter : IQueryFilter<ConcertDo>
{
    /// <summary>
    /// Search for country (in any country-related field)
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Search for exact country code
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Filter for concert before a specific date. If null, no filter is applied
    /// </summary>
    public DateTimeOffset? Before { get; set; }
    
    /// <summary>
    /// Filter for concert after a specific date. If null, no filter is applied
    /// </summary>
    public DateTimeOffset? After { get; set; }
    
    public IQueryable<ConcertDo> ApplyTo(IQueryable<ConcertDo> query)
    {
        // Filter for country (in any country-related field)
        if (!string.IsNullOrEmpty(Country))
        {
            query = query.Where(c => 
                EF.Functions.Like(c.Venue.Country.Name, $"%{Country}%")
                || EF.Functions.Like(c.Venue.Country.NativeName, $"%{Country}%")
                || EF.Functions.Like(c.Venue.CountryCode, $"%{Country}%")
                );
        }
        
        // filter for exact country code
        if (!string.IsNullOrEmpty(CountryCode))
        {
            query = query.Where(c => c.Venue.CountryCode == CountryCode);
        }
        
        // filter for date range
        if (Before.HasValue)
        {
            query = query.Where(c => c.PostedStartTime <= Before.Value);
        }
        if (After.HasValue)
        {
            query = query.Where(c => c.PostedStartTime >= After.Value);
        }
        
        return query;
    }
}