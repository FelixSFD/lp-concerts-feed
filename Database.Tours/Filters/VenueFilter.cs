using Common.Database.Filter;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Filters;

/// <summary>
/// Filter for concerts
/// </summary>
public class VenueFilter : IQueryFilter<VenueDo>
{
    /// <summary>
    /// Search for country name or code
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Search for exact country code
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Search for city name
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Search for venue name (including previous names)
    /// </summary>
    public string? Venue { get; set; }
    
    public IQueryable<VenueDo> ApplyTo(IQueryable<VenueDo> query)
    {
        // Filter for country (in any country-related field)
        if (!string.IsNullOrEmpty(Country))
        {
            query = query.Where(v => 
                EF.Functions.Like(v.Country.Name, $"%{Country}%")
                || EF.Functions.Like(v.Country.NativeName, $"%{Country}%")
                || EF.Functions.Like(v.CountryCode, $"%{Country}%")
            );
        }
        
        // filter for exact country code
        if (!string.IsNullOrEmpty(CountryCode))
        {
            query = query.Where(v => v.CountryCode == CountryCode);
        }
        
        // filter for city name
        if (!string.IsNullOrEmpty(City))
        {
            query = query.Where(v => 
                EF.Functions.Like(v.City.Name, $"%{City}%")
                || EF.Functions.Like(v.City.NativeName, $"%{City}%")
            );
        }
        
        // filter for venue name
        if (!string.IsNullOrEmpty(Venue))
        {
            query = query.Where(v => 
                EF.Functions.Like(v.CurrentName, $"%{Venue}%")
                || v.PreviousNames.Any(vn => EF.Functions.Like(vn.Name, $"%{Venue}%"))
            );
        }
        
        return query;
    }
}