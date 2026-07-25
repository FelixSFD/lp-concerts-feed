namespace Service.Tours.Exceptions;

public class ConcertNotFoundException(string concertId) : NotFoundExceptionBase("Concert", concertId)
{
    /// <summary>
    /// ID of the concert
    /// </summary>
    public string ConcertId { get; } = concertId;
}