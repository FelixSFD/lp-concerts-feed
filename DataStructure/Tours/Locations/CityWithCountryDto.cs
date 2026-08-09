namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// City with the information about the country and state the city is located in
/// </summary>
public class CityWithCountryDto : CityDto
{
    /// <summary>
    /// Information about the country
    /// </summary>
    public required CountryBo Country { get; set; }
    
    /// <summary>
    /// Information about the state, if <see cref="CityDto.StateCode"/> is set.
    /// </summary>
    public StateDto? State { get; set; }
}