namespace Service.Tours.Exceptions;

public class CountryNotFoundException(string countryCode) : NotFoundExceptionBase("Country", countryCode)
{
    /// <summary>
    /// ISO Code of the country that could not be found
    /// </summary>
    public string CountryCode { get; } = countryCode;
}