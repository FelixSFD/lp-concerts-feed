using LPCalendar.DataStructure.Requests;

namespace LPCalendar.DataStructure.Tours;

public class GetConcertsFilterDto : BaseFilterQuery
{
    /// <summary>
    /// Filter for a country by its 3-letter ISO-code
    /// </summary>
    public string? CountryCode { get; set; }
}