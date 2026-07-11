namespace Service.Tours.Exceptions;

public class CountryNotFoundException(string countryCode) : Exception($"Country '{countryCode}' not found!")
{
}