using Database.Tours.DataObjects;
using LPCalendar.DataStructure.Tours;
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
    /// Fills the <see cref="CountryDo"/> with updated information from a <see cref="UpdateCountryRequestDto"/>
    /// </summary>
    /// <param name="dataObject">Object to update</param>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the updated object</returns>
    public static CountryDo UpdateFromRequestDto(this CountryDo dataObject, UpdateCountryRequestDto dto)
    {
        dataObject.Name = dto.Name;
        dataObject.NativeName = dto.NativeName;
        return dataObject;
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
    /// Fills the <see cref="StateDo"/> with updated information from a <see cref="UpdateStateRequestDto"/>
    /// </summary>
    /// <param name="dataObject">Object to update</param>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the updated object</returns>
    public static StateDo UpdateFromRequestDto(this StateDo dataObject, UpdateStateRequestDto dto)
    {
        dataObject.Name = dto.Name;
        dataObject.NativeName = dto.NativeName;
        return dataObject;
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
    /// Fills the <see cref="CityDo"/> with updated information from a <see cref="UpdateCityRequestDto"/>
    /// </summary>
    /// <param name="dataObject">Object to update</param>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the updated object</returns>
    public static CityDo UpdateFromRequestDto(this CityDo dataObject, UpdateCityRequestDto dto)
    {
        dataObject.StateCode = dto.StateCode;
        dataObject.Name = dto.Name;
        dataObject.NativeName = dto.NativeName;
        return dataObject;
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
    
    /// <summary>
    /// Converts a <see cref="VenueDo"/> to the <see cref="VenueWithCityDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static VenueWithCityDto ToDtoWithCityDetails(this VenueDo dataObject)
    {
        return new VenueWithCityDto
        {
            Id = dataObject.Id,
            CountryCode = dataObject.CountryCode,
            StateCode = dataObject.StateCode,
            CityId = dataObject.CityId,
            CurrentName = dataObject.CurrentName,
            TimeZone = dataObject.TimeZone,
            Latitude = dataObject.Latitude,
            Longitude = dataObject.Longitude,
            City = dataObject.City.ToDtoWithCountry(),
        };
    }
    
    /// <summary>
    /// Converts a <see cref="VenueDo"/> to the <see cref="VenueWithDetailsDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static VenueWithDetailsDto ToDtoWithAllDetails(this VenueDo dataObject)
    {
        return new VenueWithDetailsDto
        {
            Id = dataObject.Id,
            CountryCode = dataObject.CountryCode,
            StateCode = dataObject.StateCode,
            CityId = dataObject.CityId,
            CurrentName = dataObject.CurrentName,
            TimeZone = dataObject.TimeZone,
            Latitude = dataObject.Latitude,
            Longitude = dataObject.Longitude,
            City = dataObject.City.ToDtoWithCountry(),
            VenueNames = dataObject.PreviousNames.Select(ToDto).ToArray(),
        };
    }

    /// <summary>
    /// Updates the properties of the <see cref="VenueDo"/> with the information in the <param name="updateRequest"></param>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="updateRequest">Information to update the DataObject</param>
    /// <returns>the updated object</returns>
    public static VenueDo UpdateFromRequestDto(this VenueDo dataObject, UpdateVenueRequestDto updateRequest)
    {
        dataObject.CountryCode = updateRequest.CountryCode;
        dataObject.StateCode = updateRequest.StateCode;
        dataObject.CityId = updateRequest.CityId;
        dataObject.Latitude = updateRequest.Latitude ?? 0;
        dataObject.Longitude = updateRequest.Longitude ?? 0;
        dataObject.TimeZone = updateRequest.TimeZone;
        return dataObject;
    }
    
    /// <summary>
    /// Converts a <see cref="PreviousVenueNameDo"/> to the <see cref="PreviousVenueNameDto"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static PreviousVenueNameDto ToDto(this PreviousVenueNameDo dataObject)
    {
        return new PreviousVenueNameDto
        {
            Id = dataObject.Id,
            VenueId = dataObject.VenueId,
            Name = dataObject.Name,
            UsedFrom = dataObject.From,
            UsedUntil = dataObject.To,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="AddVenueNameRequestDto"/> to the <see cref="PreviousVenueNameDo"/>
    /// </summary>
    /// <param name="request">DataObject to convert to the DTO</param>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>the mapped DataObject</returns>
    public static PreviousVenueNameDo ToDo(this AddVenueNameRequestDto request, uint venueId)
    {
        return new PreviousVenueNameDo
        {
            VenueId = venueId,
            Name = request.Name,
            From = request.From,
            To = request.To,
        };
    }

    /// <summary>
    /// Creates a new <see cref="TourDo"/> from a <see cref="CreateTourRequestDto"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>the new data object</returns>
    public static TourDo ToDo(this CreateTourRequestDto dto)
    {
        return new TourDo
        {
            Id = dto.Id,
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="TourDto"/> from a <see cref="TourDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static TourDto ToDto(this TourDo dataObject)
    {
        return new TourDto
        {
            Id = dataObject.Id,
            Name = dataObject.Name,
            Legs = [.. dataObject.Legs.Select(ToDto)],
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="TourLegDo"/> from a <see cref="AddTourLegRequestDto"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="tourId">ID of the tour that contains this new leg</param>
    /// <returns>the new data object</returns>
    public static TourLegDo ToDo(this AddTourLegRequestDto dto, string tourId)
    {
        return new TourLegDo
        {
            TourId = tourId,
            Id = dto.Id,
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="TourLegDto"/> from a <see cref="TourLegDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static TourLegDto ToDto(this TourLegDo dataObject)
    {
        return new TourLegDto
        {
            TourId = dataObject.TourId,
            Id = dataObject.Id,
            Name = dataObject.Name,
        };
    }

    /// <summary>
    /// Creates a new <see cref="ConcertTypeDo"/> from a <see cref="CreateConcertTypeRequestDto"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>the new data object</returns>
    public static ConcertTypeDo ToDo(this CreateConcertTypeRequestDto dto)
    {
        return new ConcertTypeDo
        {
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Updates the properties of the <see cref="ConcertTypeDo"/> with the information in the <param name="updateRequest"></param>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="updateRequest">Information to update the DataObject</param>
    /// <returns>the updated object</returns>
    public static ConcertTypeDo UpdateFromRequestDto(this ConcertTypeDo dataObject, UpdateConcertTypeRequestDto updateRequest)
    {
        dataObject.Name = updateRequest.Name;
        return dataObject;
    }
    
    /// <summary>
    /// Creates a new <see cref="TourLegDto"/> from a <see cref="ConcertTypeDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static ConcertTypeDto ToDto(this ConcertTypeDo dataObject)
    {
        return new ConcertTypeDto
        {
            Id = dataObject.Id,
            Name = dataObject.Name,
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="ConcertDo"/> from a <see cref="CreateConcertRequestDto"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>the new data object</returns>
    public static ConcertDo ToDo(this CreateConcertRequestDto dto)
    {
        return new ConcertDo
        {
            Id = Guid.NewGuid().ToString(),
            ConcertTypeId = dto.ConcertTypeId,
            TourId = dto.TourId,
            TourLegId = dto.TourLegId,
            CustomTitle = dto.CustomTitle,
            VenueId = dto.VenueId,
            PostedStartTime = dto.PostedStartTime,
            MainStageTime = dto.MainStageTime,
            DoorsTime = dto.DoorsTime,
            LpuEarlyEntryTime = dto.LpuEarlyEntryTime,
            LpuEarlyEntryConfirmed = dto.LpuEarlyEntryConfirmed,
            ExpectedSetDurationMinutes = dto.ExpectedSetDurationMinutes,
            ScheduleImageFile = dto.ScheduleImageFile,
            DeletedAt = dto.DeletedAt,
            //Status = dto.Status,
        };
    }
    
    /// <summary>
    /// Creates a <see cref="RawConcertDto"/> from the <see cref="ConcertDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static RawConcertDto ToDto(this ConcertDo dataObject)
    {
        return new RawConcertDto
        {
            Id = dataObject.Id,
            ConcertTypeId = dataObject.ConcertTypeId,
            TourId = dataObject.TourId,
            TourLegId = dataObject.TourLegId,
            CustomTitle = dataObject.CustomTitle,
            VenueId = dataObject.VenueId,
            PostedStartTime = dataObject.PostedStartTime,
            MainStageTime = dataObject.MainStageTime,
            DoorsTime = dataObject.DoorsTime,
            LpuEarlyEntryTime = dataObject.LpuEarlyEntryTime,
            LpuEarlyEntryConfirmed = dataObject.LpuEarlyEntryConfirmed,
            ExpectedSetDurationMinutes = dataObject.ExpectedSetDurationMinutes,
            ScheduleImageFile = dataObject.ScheduleImageFile,
            DeletedAt = dataObject.DeletedAt,
            //Status = dto.Status,
        };
    }
    
    /// <summary>
    /// Creates a <see cref="ConcertDetailsDto"/> from the <see cref="ConcertDo"/> including all references.
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static ConcertDetailsDto ToDtoWithDetails(this ConcertDo dataObject)
    {
        return new ConcertDetailsDto
        {
            Id = dataObject.Id,
            ConcertType = dataObject.Type.ToDto(),
            Tour = dataObject.Tour?.ToDto(),
            TourLeg = dataObject.TourLeg?.ToDto(),
            CustomTitle = dataObject.CustomTitle,
            Venue = dataObject.Venue.ToDtoWithAllDetails(),
            PostedStartTime = dataObject.PostedStartTime,
            MainStageTime = dataObject.MainStageTime,
            DoorsTime = dataObject.DoorsTime,
            LpuEarlyEntryTime = dataObject.LpuEarlyEntryTime,
            LpuEarlyEntryConfirmed = dataObject.LpuEarlyEntryConfirmed,
            ExpectedSetDurationMinutes = dataObject.ExpectedSetDurationMinutes,
            ScheduleImageFile = dataObject.ScheduleImageFile,
            DeletedAt = dataObject.DeletedAt,
            //Status = dto.Status,
        };
    }
}