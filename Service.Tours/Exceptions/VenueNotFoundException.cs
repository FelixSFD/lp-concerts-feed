namespace Service.Tours.Exceptions;

public class VenueNotFoundException(uint venueId) : NotFoundExceptionBase("Venue", venueId.ToString())
{
    /// <summary>
    /// ID of the venue
    /// </summary>
    public uint VenueId { get; } = venueId;
}