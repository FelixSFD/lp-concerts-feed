using LPCalendar.DataStructure.Requests;

namespace LPCalendar.DataStructure.Tours;

public class GetVenuesFilterDto : BaseFilterQuery
{
    /// <summary>
    /// Filter for a country by its 3-letter ISO-code
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Filter for a venue by its name (including previous names)
    /// </summary>
    public string? Venue { get; set; }

    public override string[] OrderBy { get; set; } = ["countryCode", "venue"];
}