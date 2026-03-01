using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using LPCalendar.DataStructure;

namespace Lambda.CalendarFeed;

/// <summary>
/// Helper functions and extensions to work with Calendar entries
/// </summary>
public static class CalendarHelper
{
    public static CalDateTime ToCalDateTime(this DateTimeOffset dateTimeOffset, string timezoneId)
    {
        return new CalDateTime(dateTimeOffset.ConvertToTimezone(timezoneId).DateTime, timezoneId);
    }
    
    
    public static DateTimeOffset ConvertToTimezone(this DateTimeOffset dateTimeOffset, string timezoneId)
    {
        var targetTz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return TimeZoneInfo.ConvertTime(dateTimeOffset, targetTz);
    }


    /// <summary>
    /// Returns the coordinates of the venue as <see cref="GeographicLocation"/>
    /// </summary>
    /// <param name="concert"></param>
    /// <returns>Geographic location or null, if the coordinates are not set in the <see cref="Concert"/></returns>
    internal static GeographicLocation? GetGeoLocation(this Concert concert)
    {
        if (concert.VenueLatitude != 0 || concert.VenueLongitude != 0)
        {
            // coordinates exist -> return location
            return new GeographicLocation
            {
                Latitude = (double)concert.VenueLatitude,
                Longitude = (double)concert.VenueLongitude
            };
        }

        // coordinates were not set, so we can't return a location
        return null;
    }


    /// <summary>
    /// Converts the <see cref="Concert"/>s from the enumerable to one or more <see cref="CalendarEvent"/>s each.
    /// </summary>
    /// <param name="concerts">input concerts</param>
    /// <param name="categoryFlags">Options about which events to create</param>
    /// <returns>Events for the iCal feed</returns>
    public static IEnumerable<CalendarEvent> ToCalendarEvents(this IEnumerable<Concert> concerts,
        ConcertSubEventCategory categoryFlags = ConcertSubEventCategory.AsOneSingleEvent)
    {
        return concerts
            .SelectMany(c => ToCalendarEvents(c, categoryFlags))
            .Where(c => c != null)
            .Cast<CalendarEvent>();
    }
    
    
    /// <summary>
    /// Returns one or more <see cref="CalendarEvent"/>s for a <see cref="Concert"/>.
    /// This depends on the settings whether a detailed calendar is requested.
    /// </summary>
    /// <param name="concert">Concert to create the events for</param>
    /// <param name="categoryFlags">Flags to specify which sub-events to return for the concert</param>
    /// <returns>one or more events</returns>
    private static IEnumerable<CalendarEvent?> ToCalendarEvents(Concert concert, ConcertSubEventCategory categoryFlags = ConcertSubEventCategory.AsOneSingleEvent)
    {
        if (categoryFlags.HasFlag(ConcertSubEventCategory.AsOneSingleEvent))
        {
            yield return GetFullEventFor(concert);
        }
        else
        {
            // More detailed calendar was requested -> Create events for every part of the Concert (if available)

            CalendarEvent? lpStageTimeEvent = null;
            CalendarEvent? doorsTimeEvent = null;
            if (categoryFlags.HasFlag(ConcertSubEventCategory.LinkinPark))
            {
                lpStageTimeEvent = GetEventForLinkinParkStageTime(concert);
            }
            
            if (categoryFlags.HasFlag(ConcertSubEventCategory.Doors))
            {
                doorsTimeEvent = GetEventForDoorsTime(concert, lpStageTimeEvent?.Start);
            }

            if (lpStageTimeEvent == null && doorsTimeEvent == null)
            {
                // no detailed information seem to be available -> return full event. Otherwise, the cal would be empty
                yield return GetFullEventFor(concert);
            }
            else
            {
                // details seem to be available -> return detailed events
                yield return doorsTimeEvent;
                yield return lpStageTimeEvent;
            }
        }
    }
    
    
    /// <summary>
    /// Returns the <see cref="CalendarEvent"/> for a <see cref="Concert"/> as one entry for the full show
    /// </summary>
    /// <param name="concert">Concert to generate the event for</param>
    /// <returns>Calendar Event</returns>
    public static CalendarEvent? GetFullEventFor(Concert concert)
    {
        return concert.TourName != null ? GetCalendarEventWithTourName(concert) : GetCalendarEventWithoutTourName(concert);
    }


    /// <summary>
    /// Returns a <see cref="CalendarEvent"/> for the time when Linkin Park is scheduled to play
    /// </summary>
    /// <param name="concert"></param>
    /// <returns>event or null, if the <see cref="Concert.MainStageTime"/> is not set</returns>
    private static CalendarEvent? GetEventForLinkinParkStageTime(Concert concert)
    {
        if (concert.MainStageTime == null) 
            return null;
        
        var title = GetStageTimeTitle(concert);
        var description = $"Stage time for Linkin Park at {concert.Venue}.\nType of show: {concert.ShowType}{GetAppHintString(concert)}";

            
        var date = concert.MainStageTime.Value.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            Summary = title,
            Description = description,
            Location = $"{concert.LocationLong}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = date,
            End = date.AddMinutes(concert.ExpectedSetDuration ?? 120),
            //Duration = Duration.FromMinutes(concert.ExpectedSetDuration ?? 120),
            LastModified = concert.LastChange?.ToCalDateTime(concert.TimeZoneId)
        };
        
        var url = GetUrlToConcert(concert);
        if (url != null)
        {
            calendarEvent.Url = url;
        }

        return calendarEvent;
    }
    
    
    /// <summary>
    /// Returns a <see cref="CalendarEvent"/> for the time when the doors will open
    /// </summary>
    /// <param name="concert"></param>
    /// <param name="nextEventStart"></param>
    /// <returns>event or null, if the <see cref="Concert.DoorsTime"/> is not set</returns>
    private static CalendarEvent? GetEventForDoorsTime(Concert concert, CalDateTime? nextEventStart)
    {
        if (concert.DoorsTime == null) 
            return null;
        
        var title = $"Doors open: Linkin Park in {concert.City}";
        var description = $"Doors open at {concert.Venue} for the Linkin Park concert{GetAppHintString(concert)}";

        var date = concert.DoorsTime.Value.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            Summary = title,
            Description = description,
            Location = $"{concert.LocationLong}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = date,
            End = nextEventStart,
            LastModified = concert.LastChange?.ToCalDateTime(concert.TimeZoneId)
        };
        
        var url = GetUrlToConcert(concert);
        if (url != null)
        {
            calendarEvent.Url = url;
        }

        return calendarEvent;
    }
    
    
    private static CalendarEvent? GetCalendarEventWithTourName(Concert concert)
    {
        if (concert.PostedStartTime == null)
            return null;

        var title = GetConcertTitle(concert);
        var description = $"Concert of the Linkin Park {concert.TourName}{GetAppHintString(concert)}";

        CalDateTime? startDate;
        Duration duration;
        if (concert.MainStageTime != null)
        {
            // if available, use main stage time. In that case, decrease the duration of the event
            startDate = concert.MainStageTime?.ToCalDateTime(concert.TimeZoneId);
            duration = Duration.FromHours(2);
        }
        else
        {
            // only start time from ticket available
            startDate = concert.PostedStartTime?.ToCalDateTime(concert.TimeZoneId);
            duration = Duration.FromHours(3);
        }
        
        var endDate = startDate?.Add(duration);
        
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = title,
            Description = description,
            Location = $"{concert.LocationLong}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = startDate,
            End = endDate,
            LastModified = concert.LastChange?.ToCalDateTime(concert.TimeZoneId)
        };
        
        var url = GetUrlToConcert(concert);
        if (url != null)
        {
            calendarEvent.Url = url;
        }

        return calendarEvent;
    }
    
    
    private static CalendarEvent? GetCalendarEventWithoutTourName(Concert concert)
    {
        if (concert.PostedStartTime == null)
            return null;
        
        var title = GetConcertTitle(concert);
        var description = $"Linkin Park Concert at {concert.Venue}\nThis show is not part of a tour.{GetAppHintString(concert)}";

        CalDateTime? startDate;
        Duration duration;
        if (concert.MainStageTime != null)
        {
            // if available, use main stage time. In that case, decrease the duration of the event
            startDate = concert.MainStageTime?.ToCalDateTime(concert.TimeZoneId);
            duration = Duration.FromHours(2);
        }
        else
        {
            // only start time from ticket available
            startDate = concert.PostedStartTime?.ToCalDateTime(concert.TimeZoneId);
            duration = Duration.FromHours(3);
        }
        
        var endDate = startDate?.Add(duration);
        
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = title,
            Description = description,
            Location = $"{concert.LocationMedium}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = startDate,
            End = endDate,
            LastModified = concert.LastChange?.ToCalDateTime(concert.TimeZoneId)
        };
        
        var url = GetUrlToConcert(concert);
        if (url != null)
        {
            calendarEvent.Url = url;
        }

        return calendarEvent;
    }


    /// <summary>
    /// Returns the title of the concert
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    private static string GetConcertTitle(Concert concert)
    {
        if (!string.IsNullOrEmpty(concert.CustomTitle))
        {
            return concert.CustomTitle;
        }

        if (concert.ShowType == "Linkin Park Show" && !string.IsNullOrEmpty(concert.TourName))
        {
            return $"{concert.TourName}: {concert.City}";
        }

        return $"Linkin Park: {concert.Venue}";
    }
    
    
    /// <summary>
    /// Returns the title of the concert
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    private static string GetStageTimeTitle(Concert concert)
    {
        if (!string.IsNullOrEmpty(concert.CustomTitle))
        {
            return concert.CustomTitle;
        }

        if (concert.ShowType == "Linkin Park Show" && !string.IsNullOrEmpty(concert.TourName))
        {
            return $"{concert.TourName}: {concert.City} (Stage Time)";
        }

        return $"Linkin Park: {concert.City} (Stage Time)";
    }


    /// <summary>
    /// Returns a link to the concert. This relies on the Env-Variable ROOT_DOMAIN to be set!
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    private static Uri? GetUrlToConcert(Concert concert)
    {
        var rootDomainStr = Environment.GetEnvironmentVariable("ROOT_DOMAIN");
        return !string.IsNullOrEmpty(rootDomainStr) ? new Uri($"https://{rootDomainStr}/concert/{concert.Id}") : null;
    }


    /// <summary>
    /// Returns a string with an information about the app
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    private static string GetAppHintString(Concert concert)
    {
        var rootDomainStr = Environment.GetEnvironmentVariable("ROOT_DOMAIN");
        if (string.IsNullOrEmpty(rootDomainStr))
        {
            return string.Empty;
        }
        
        return $"\n\nTry our new FREE app: https://{rootDomainStr}/app?mtm_kwd={concert.Id}&mtm_campaign=ical-feed";
    }
}