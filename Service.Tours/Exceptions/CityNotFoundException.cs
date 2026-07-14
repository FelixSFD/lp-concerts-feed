namespace Service.Tours.Exceptions;

public class CityNotFoundException(uint cityId) : NotFoundExceptionBase("City", cityId.ToString())
{
    /// <summary>
    /// ID of the city
    /// </summary>
    public uint CityId { get; } = cityId;
}