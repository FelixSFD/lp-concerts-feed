namespace Service.Setlists.Exceptions;

public class ConcertNotFoundException(string concertId)
    : SetlistServiceException($"The concert with id '{concertId}' was not found.")
{
    /// <summary>
    /// ID of the concert
    /// </summary>
    public string ConcertId { get; set; } = concertId;
}