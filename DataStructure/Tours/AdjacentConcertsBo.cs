namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Information about the previous and the next concert after a given concert
/// </summary>
public class AdjacentConcertsBo
{
    /// <summary>
    /// Concert before the given concert
    /// </summary>
    public ConcertDetailsBo? Previous { get; set; }
    
    /// <summary>
    /// Next concert after the given concert
    /// </summary>
    public ConcertDetailsBo? Next { get; set; }
}