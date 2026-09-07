using LPCalendar.DataStructure.Tours;
using LPCalendar.DataStructure.Tours.Locations;

namespace Service.Tours.DataStructure;

/// <summary>
/// The plan for a concert that is about to be imported.
/// This can have objects that already exist in the database or the necessary data to create them.
/// </summary>
public class ImportConcertPreviewBo
{
    public ConcertTypeBo? ConcertType { get; set; }
    
    public DateTimeOffset PostedStartTime { get; set; }
    
    /// <summary>
    /// If matching cities were found, they are listed here
    /// </summary>
    public required CityWithCountryBo[] FoundCites { get; set; }
    
    public required VenueBo[] FoundVenues { get; set; }
    public required CountryBo[] FoundCountries { get; set; }
    public required StateBo[] FoundStates { get; set; }

    public string? CountryName { get; set; }
    public string? StateName { get; set; }
    public string? CityName { get; set; }
    public string? VenueName { get; set; }
    
    public required TourBo[] FoundTours { get; set; }
    public required TourLegBo[] FoundTourLegs { get; set; }

    public string? TourName { get; set; }
    public string? TourLegName { get; set; }

    public string? ProposedCustomTitle { get; set; }
}