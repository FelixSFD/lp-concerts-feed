namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Basic information about a tour
/// </summary>
public class TourBo
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Name of this tour
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Legs of this tour. Not all tours might be split into different legs.
    /// </summary>
    public ICollection<TourLegBo> Legs { get; set; } = [];
}