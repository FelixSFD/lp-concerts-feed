namespace Service.Tours.Exceptions;

public class VenueNameNotFoundException(uint venueId, uint venueNameId) : NotFoundExceptionBase("Venue Name", venueId.ToString(), venueNameId.ToString())
{
    /// <summary>
    /// ID of the venue
    /// </summary>
    public uint VenueId { get; } = venueId;
}