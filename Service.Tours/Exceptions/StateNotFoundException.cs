namespace Service.Tours.Exceptions;

public class StateNotFoundException(string countryCode, string stateCode) : NotFoundExceptionBase("State", countryCode, stateCode)
{
    /// <summary>
    /// ISO Code of the country that could not be found
    /// </summary>
    public string CountryCode { get; } = countryCode;
    
    /// <summary>
    /// Code of the state in the country
    /// </summary>
    public string StateCode { get; } = stateCode;
}