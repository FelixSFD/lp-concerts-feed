

namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// A state within a country including the data of the country
/// </summary>
public class StateWithCountryDto : StateDto
{
    /// <summary>
    /// Country the state is in
    /// </summary>
    public required CountryBo Country { get; set; }
}