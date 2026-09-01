using Database.Tours.DataObjects;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Tours;
using LPCalendar.DataStructure.Tours.Locations;

namespace Service.Tours;

internal static class DoMapper
{
    /// <summary>
    /// Converts a <see cref="CountryDo"/> to the <see cref="CountryBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static CountryBo ToBo(this CountryDo dataObject)
    {
        return new CountryBo
        {
            IsoCode = dataObject.IsoCode,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CountryBo"/> to the <see cref="CountryDo"/>
    /// </summary>
    /// <param name="bo">DTO to convert to the DataObject</param>
    /// <returns>the mapped DTO</returns>
    public static CountryDo ToDo(this CountryBo bo)
    {
        return new CountryDo
        {
            IsoCode = bo.IsoCode,
            Name = bo.Name,
            NativeName = bo.NativeName,
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CreateCountryRequest"/> to the <see cref="CountryDo"/>
    /// </summary>
    /// <param name="dto">DTO to convert to the DataObject</param>
    /// <returns>the mapped DTO</returns>
    public static CountryDo ToDo(this CreateCountryRequest dto)
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
    /// Converts a <see cref="StateDo"/> to the <see cref="StateWithCountryBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static StateWithCountryBo ToDtoWithCountry(this StateDo dataObject)
    {
        return new StateWithCountryBo
        {
            CountryCode = dataObject.CountryCode,
            Code = dataObject.Code,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
            Country = dataObject.Country.ToBo()
        };
    }
    
    /// <summary>
    /// Converts a <see cref="StateDo"/> to the <see cref="StateBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static StateBo ToBo(this StateDo dataObject)
    {
        return new StateBo
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
    /// Converts a <see cref="CityDo"/> to the <see cref="CityWithCountryBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static CityWithCountryBo ToDtoWithCountry(this CityDo dataObject)
    {
        return new CityWithCountryBo
        {
            Id = dataObject.Id,
            CountryCode = dataObject.CountryCode,
            StateCode = dataObject.StateCode,
            Name = dataObject.Name,
            NativeName = dataObject.NativeName,
            Country = dataObject.Country.ToBo(),
            State = dataObject.State?.ToBo()
        };
    }
    
    /// <summary>
    /// Converts a <see cref="CityDo"/> to the <see cref="CityBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static CityBo ToBo(this CityDo dataObject)
    {
        return new CityBo
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
    /// Converts a <see cref="VenueDo"/> to the <see cref="VenueBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static VenueBo ToBo(this VenueDo dataObject)
    {
        return new VenueBo
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
    /// Converts a <see cref="VenueDo"/> to the <see cref="VenueWithCityBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static VenueWithCityBo ToBoWithCityDetails(this VenueDo dataObject)
    {
        return new VenueWithCityBo
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
    /// Converts a <see cref="VenueDo"/> to the <see cref="VenueWithDetailsBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static VenueWithDetailsBo ToBoWithAllDetails(this VenueDo dataObject)
    {
        return new VenueWithDetailsBo
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
            VenueNames = dataObject.PreviousNames.Select(ToBo).ToArray(),
        };
    }

    /// <summary>
    /// Updates the properties of the <see cref="VenueDo"/> with the information in the <param name="updateRequest"></param>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="updateRequest">Information to update the DataObject</param>
    /// <returns>the updated object</returns>
    public static VenueDo UpdateFromRequestBo(this VenueDo dataObject, UpdateVenueRequestBo updateRequest)
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
    /// Converts a <see cref="PreviousVenueNameDo"/> to the <see cref="PreviousVenueNameBo"/>
    /// </summary>
    /// <param name="dataObject">DataObject to convert to the DTO</param>
    /// <returns>the mapped DTO</returns>
    public static PreviousVenueNameBo ToBo(this PreviousVenueNameDo dataObject)
    {
        return new PreviousVenueNameBo
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
    /// Creates a new <see cref="TourDo"/> from a <see cref="CreateTourRequest"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>the new data object</returns>
    public static TourDo ToDo(this CreateTourRequest dto)
    {
        return new TourDo
        {
            Id = dto.Id,
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="TourBo"/> from a <see cref="TourDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static TourBo ToBo(this TourDo dataObject)
    {
        return new TourBo
        {
            Id = dataObject.Id,
            Name = dataObject.Name,
            Legs = [.. dataObject.Legs.Select(ToBo)],
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="TourLegDo"/> from a <see cref="AddTourLegRequest"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="tourId">ID of the tour that contains this new leg</param>
    /// <returns>the new data object</returns>
    public static TourLegDo ToDo(this AddTourLegRequest dto, string tourId)
    {
        return new TourLegDo
        {
            TourId = tourId,
            Id = dto.Id,
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="TourLegBo"/> from a <see cref="TourLegDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static TourLegBo ToBo(this TourLegDo dataObject)
    {
        return new TourLegBo
        {
            TourId = dataObject.TourId,
            Id = dataObject.Id,
            Name = dataObject.Name,
        };
    }

    /// <summary>
    /// Creates a new <see cref="ConcertTypeDo"/> from a <see cref="CreateConcertTypeRequest"/>
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>the new data object</returns>
    public static ConcertTypeDo ToDo(this CreateConcertTypeRequest dto)
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
    public static ConcertTypeDo UpdateFromRequestDto(this ConcertTypeDo dataObject, UpdateConcertTypeRequest updateRequest)
    {
        dataObject.Name = updateRequest.Name;
        return dataObject;
    }
    
    /// <summary>
    /// Creates a new <see cref="TourLegBo"/> from a <see cref="ConcertTypeDo"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static ConcertTypeBo ToBo(this ConcertTypeDo dataObject)
    {
        return new ConcertTypeBo
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
            //Status = dto.Status,
        };
    }
    
    /// <summary>
    /// Updates the <see cref="ConcertDo"/> from a <see cref="UpdateConcertRequestDto"/>
    /// </summary>
    /// <param name="dataObject"></param>
    /// <param name="dto"></param>
    /// <returns>the new data object</returns>
    public static ConcertDo UpdateFromRequestDto(this ConcertDo dataObject, UpdateConcertRequestDto dto)
    {
        dataObject.ConcertTypeId = dto.ConcertTypeId;
        dataObject.TourId = dto.TourId;
        dataObject.TourLegId = dto.TourLegId;
        dataObject.CustomTitle = dto.CustomTitle;
        dataObject.VenueId = dto.VenueId;
        dataObject.PostedStartTime = dto.PostedStartTime;
        dataObject.MainStageTime = dto.MainStageTime;
        dataObject.DoorsTime = dto.DoorsTime;
        dataObject.LpuEarlyEntryTime = dto.LpuEarlyEntryTime;
        dataObject.LpuEarlyEntryConfirmed = dto.LpuEarlyEntryConfirmed;
        dataObject.ExpectedSetDurationMinutes = dto.ExpectedSetDurationMinutes;
        dataObject.ScheduleImageFile = dto.ScheduleImageFile;
        dataObject.DeletedAt = dto.DeletedAt;
        //dataObject.Status = dto.Status;
        return dataObject;
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
    /// Creates a <see cref="ConcertDetailsBo"/> from the <see cref="ConcertDo"/> including all references.
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns>the new DTO</returns>
    public static ConcertDetailsBo ToBoWithDetails(this ConcertDo dataObject)
    {
        return new ConcertDetailsBo
        {
            Id = dataObject.Id,
            ConcertType = dataObject.Type.ToBo(),
            Tour = dataObject.Tour?.ToBo(),
            TourLeg = dataObject.TourLeg?.ToBo(),
            CustomTitle = dataObject.CustomTitle,
            Venue = dataObject.Venue.ToBoWithAllDetails(),
            PostedStartTime = dataObject.PostedStartTime,
            MainStageTime = dataObject.MainStageTime,
            DoorsTime = dataObject.DoorsTime,
            LpuEarlyEntryTime = dataObject.LpuEarlyEntryTime,
            LpuEarlyEntryConfirmed = dataObject.LpuEarlyEntryConfirmed,
            ExpectedSetDurationMinutes = dataObject.ExpectedSetDurationMinutes,
            ScheduleImageFile = dataObject.ScheduleImageFile,
            DeletedAt = dataObject.DeletedAt,
            Status = dataObject.Status.ToDto(),
        };
    }

    public static ConcertDto.ConcertStatusValue ToDto(this ConcertDo.ConcertStatus data)
    {
        return data switch
        {
            ConcertDo.ConcertStatus.Planned => ConcertDto.ConcertStatusValue.Planned,
            ConcertDo.ConcertStatus.LiveRightNow => ConcertDto.ConcertStatusValue.Running,
            ConcertDo.ConcertStatus.Past => ConcertDto.ConcertStatusValue.Past,
            ConcertDo.ConcertStatus.Cancelled => ConcertDto.ConcertStatusValue.Cancelled,
            _ => ConcertDto.ConcertStatusValue.Past
        };
    }
}