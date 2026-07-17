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
    /// Converts a <see cref="StateDo"/> to the <see cref="StateWithCountryDto"/>
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
    
    /// <summary>
    /// Converts a <see cref="StateDo"/> to the <see cref="StateDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static StateDto ToDto(this StateDo dataObject)
    {
        return new StateDto
        {
            CountryCode = dataObject.CountryCode,
            Code = dataObject.Code,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CreateStateRequestDto"/> to the <see cref="StateDo"/>
    /// </summary>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <param name="countryCode">ISO code of the country where the city is located in</param>
    /// <returns>the mapped DataObject</returns>
    public static CityDo ToDo(this CreateCityRequestDto dto, string countryCode)
    {
        return new CityDo
        {
            CountryCode = countryCode,
            StateCode = dto.StateCode,
            Name = dto.Name,
            NativeName = dto.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CityDo"/> to the <see cref="CityWithCountryDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static CityWithCountryDto ToDtoWithCountry(this CityDo dataObject)
    {
        return new CityWithCountryDto
        {
            Id = dataObject.Id,
            CountryCode = dataObject.CountryCode,
            StateCode = dataObject.StateCode,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
            Country = dataObject.Country.ToDto(),
            State = dataObject.State?.ToDto()
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CityDo"/> to the <see cref="CityDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static CityDto ToDto(this CityDo dataObject)
    {
        return new CityDto
        {
            Id = dataObject.Id,
            CountryCode = dataObject.CountryCode,
            StateCode = dataObject.StateCode,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CreateVenueRequestDto"/> to the <see cref="VenueDo"/>
    /// </summary>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the mapped DataObject</returns>
    public static VenueDo ToDo(this CreateVenueRequestDto dto)
    {
        return new VenueDo
        {
            CountryCode = dto.CountryCode,
            StateCode = dto.StateCode,
            CityId = dto.CityId,
            CurrentName = dto.CurrentName,
            TimeZone = dto.TimeZone,
            Latitude = dto.Latitude ?? 0,
            Longitude = dto.Longitude ?? 0
        };
    }
    
    /// <summary>
    /// Converts a <see cref="VenueDo"/> to the <see cref="VenueDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static VenueDto ToDto(this VenueDo dataObject)
    {
        return new VenueDto
        {
            Id = dataObject.Id,
            CountryCode = dataObject.CountryCode,
            StateCode = dataObject.StateCode,
            CityId = dataObject.CityId,
            CurrentName = dataObject.CurrentName,
            TimeZone = dataObject.TimeZone,
            Latitude = dataObject.Latitude,
            Longitude = dataObject.Longitude,
        };
    }
}