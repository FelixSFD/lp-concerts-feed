namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Identifier for a city within a country
/// </summary>
public class CityIdentifierBo
{
    /// <summary>
    /// ID of the city
    /// </summary>
    public required uint Id { get; set; }

    /// <summary>
    /// ISO-alpha-3 code of the country
    /// </summary>
    public required string CountryCode { get; set; }
}
