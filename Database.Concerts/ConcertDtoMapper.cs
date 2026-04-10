using System.Text.Json;
using Database.Concerts.Models;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Setlists;

namespace Database.Concerts;

public static class ConcertDtoMapper
{
    /// <summary>
    /// Maps the <see cref="ConcertModel"/> to a <see cref="ConcertDto"/>
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static ConcertDto ToDto(ConcertModel model)
    {
        return new ConcertDto
        {
            Id = model.Id,
            City = model.City,
            Country = model.Country,
            CustomTitle = model.CustomTitle,
            DeletedAt = model.DeletedAt,
            DoorsTime = model.DoorsTime,
            ExpectedSetDuration = model.ExpectedSetDuration,
            LastChange = model.LastChange,
            LpuEarlyEntryConfirmed = model.LpuEarlyEntryConfirmed,
            LpuEarlyEntryTime = model.LpuEarlyEntryTime,
            MainStageTime = model.MainStageTime,
            PostedStartTime = model.PostedStartTime,
            Status = model.Status,
            ShowType = model.ShowType,
            ScheduleImageFile = model.ScheduleImageFile,
            State = model.State,
            TourName = model.TourName,
            TimeZoneId = model.TimeZoneId,
            Venue = model.Venue,
            VenueLatitude = model.VenueLatitude,
            VenueLongitude = model.VenueLongitude,
        };
    }
    
    
    /// <summary>
    /// Maps the <see cref="ConcertModel"/> to a <see cref="ConcertDto"/>
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static ConcertWithSetlistsDto ToDtoWithSetlists(ConcertModel model)
    {
        return new ConcertWithSetlistsDto
        {
            Id = model.Id,
            City = model.City,
            Country = model.Country,
            CustomTitle = model.CustomTitle,
            DeletedAt = model.DeletedAt,
            DoorsTime = model.DoorsTime,
            ExpectedSetDuration = model.ExpectedSetDuration,
            LastChange = model.LastChange,
            LpuEarlyEntryConfirmed = model.LpuEarlyEntryConfirmed,
            LpuEarlyEntryTime = model.LpuEarlyEntryTime,
            MainStageTime = model.MainStageTime,
            PostedStartTime = model.PostedStartTime,
            Status = model.Status,
            ShowType = model.ShowType,
            ScheduleImageFile = model.ScheduleImageFile,
            State = model.State,
            TourName = model.TourName,
            TimeZoneId = model.TimeZoneId,
            Venue = model.Venue,
            VenueLatitude = model.VenueLatitude,
            VenueLongitude = model.VenueLongitude,
            // special fields for setlists cache
            CachedSetlistsAt = model.CachedSetlistsAt,
            CachedSetlists = JsonSerializer.Deserialize(model.CachedSetlistsJson ?? "[]", SetlistDtoJsonContext.Default.ListSetlistDto) ?? [],
        };
    }
    
    
    /// <summary>
    /// Maps the <see cref="ConcertDto"/> to a <see cref="ConcertModel"/>
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static ConcertModel ToModel(ConcertDto model)
    {
        return new ConcertModel
        {
            Id = model.Id,
            City = model.City,
            Country = model.Country,
            CustomTitle = model.CustomTitle,
            DeletedAt = model.DeletedAt,
            DoorsTime = model.DoorsTime,
            ExpectedSetDuration = model.ExpectedSetDuration,
            LastChange = model.LastChange,
            LpuEarlyEntryConfirmed = model.LpuEarlyEntryConfirmed,
            LpuEarlyEntryTime = model.LpuEarlyEntryTime,
            MainStageTime = model.MainStageTime,
            PostedStartTime = model.PostedStartTime,
            Status = model.Status,
            ShowType = model.ShowType,
            ScheduleImageFile = model.ScheduleImageFile,
            State = model.State,
            TourName = model.TourName,
            TimeZoneId = model.TimeZoneId,
            Venue = model.Venue,
            VenueLatitude = model.VenueLatitude,
            VenueLongitude = model.VenueLongitude,
        };
    }
}