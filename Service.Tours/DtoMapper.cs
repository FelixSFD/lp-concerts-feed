using Database.Tours.DataObjects;
using LPCalendar.DataStructure.Tours.Locations;

namespace Service.Tours;

internal static class DtoMapper
{
    /// <summary>
    /// Converts a <see cref="CountryDo"/> to the <see cref="CountryDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static CountryDto ToDto(this CountryDo dataObject)
    {
        return new CountryDto
        {
            IsoCode = dataObject.IsoCode,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CountryDto"/> to the <see cref="CountryDo"/>
    /// </summary>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the mapped DTO</returns>
    public static CountryDo ToDto(this CountryDto dto)
    {
        return new CountryDo
        {
            IsoCode = dto.IsoCode,
            Name = dto.Name,
            NativeName = dto.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CreateCountryRequestDto"/> to the <see cref="CountryDo"/>
    /// </summary>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the mapped DTO</returns>
    public static CountryDo ToDo(this CreateCountryRequestDto dto)
    {
        return new CountryDo
        {
            IsoCode = dto.IsoCode,
            Name = dto.Name,
            NativeName = dto.NativeName,
        };
    }

    /// <summary>
    /// Converts a <see cref="CreateStateRequestDto"/> to the <see cref="StateDo"/>
    /// </summary>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <param name="countryCode">ISO code of the country where the state is located in</param>
    /// <returns>the mapped DataObject</returns>
    public static StateDo ToDo(this CreateStateRequestDto dto, string countryCode)
    {
        return new StateDo
        {
            CountryCode = countryCode,
            Code = dto.Code,
            Name = dto.Name,
            NativeName = dto.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CreateStateRequestDto"/> to the <see cref="StateDo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static StateWithCountryDto ToDtoWithCountry(this StateDo dataObject)
    {
        return new StateWithCountryDto
        {
            CountryCode = dataObject.CountryCode,
            Code = dataObject.Code,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
            Country = dataObject.Country.ToDto()
        };
    }
}