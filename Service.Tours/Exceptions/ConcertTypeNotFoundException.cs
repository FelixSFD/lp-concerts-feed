namespace Service.Tours.Exceptions;

public class ConcertTypeNotFoundException(uint typeId) : NotFoundExceptionBase("Concert Type", typeId.ToString())
{
    /// <summary>
    /// ID of the concert type
    /// </summary>
    public uint TypeId { get; } = typeId;
}