namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Information about a venue
/// </summary>
public class VenueWithCityBo : VenueBo
{
    /// <summary>
    /// City where this venue is located in. Also contains information about the country
    /// </summary>
    public required CityWithCountryBo City { get; set; }
}