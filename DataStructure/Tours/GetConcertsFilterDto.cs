using LPCalendar.DataStructure.Requests;

namespace LPCalendar.DataStructure.Tours;

public class GetConcertsFilterDto : BaseFilterQuery
{
    /// <summary>
    /// Search for exact tour ID
    /// </summary>
    public string? TourId { get; set; }
    
    /// <summary>
    /// Search for exact tour leg ID. Should be used together with TourId.
    /// </summary>
    public string? TourLegId { get; set; }
    
    /// <summary>
    /// Filter for a country by its 3-letter ISO-code
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Filter for a country by its name, native name or ISO code
    /// </summary>
    public string? Country{ get; set; }

    /// <summary>
    /// Filter for a city by its name
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Filter for a venue by its name (including previous names)
    /// </summary>
    public string? Venue { get; set; }

    /// <summary>
    /// Filter for concerts by their custom title
    /// </summary>
    public string? CustomTitle { get; set; }

    /// <summary>
    /// Filter for concerts before a date
    /// </summary>
    public DateOnly? Before { get; set; }
    
    /// <summary>
    /// Filter for concerts after a date
    /// </summary>
    public DateOnly? After { get; set; }

    public override string[] OrderBy { get; set; } = ["date"];
}