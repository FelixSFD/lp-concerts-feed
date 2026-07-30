namespace Service.Tours.Exceptions;

public class TourNotFoundException(string tourId) : NotFoundExceptionBase("Tour", tourId)
{
    /// <summary>
    /// ID of the tour
    /// </summary>
    public string TourId { get; } = tourId;
}