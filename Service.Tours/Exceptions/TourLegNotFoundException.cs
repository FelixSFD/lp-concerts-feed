namespace Service.Tours.Exceptions;

public class TourLegNotFoundException(string tourId, string legId) : NotFoundExceptionBase("Tour Leg", tourId, legId)
{
    /// <summary>
    /// ID of the tour
    /// </summary>
    public string TourId { get; } = tourId;
    
    /// <summary>
    /// ID of the tour leg
    /// </summary>
    public string LegId { get; } = legId;
}