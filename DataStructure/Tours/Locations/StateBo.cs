namespace LPCalendar.DataStructure.Tours.Locations;

public class StateBo
{
    public required string CountryCode { get; set; }
    public required string Code { get; set; }

    /// <summary>
    /// English name of this state
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// name of this state in its native language
    /// </summary>
    public required string NativeName { get; set; }
}