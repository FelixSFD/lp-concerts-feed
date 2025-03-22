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

            yield return doorsTimeEvent;
            yield return lpStageTimeEvent;
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
        
        var title = $"Linkin Park: {concert.City}";
        var description = $"Stage time for Linkin Park at {concert.Venue}";

            
        var date = concert.MainStageTime.Value.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            Summary = title,
            Description = description,
            Location = $"{concert.LocationLong}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = date,
            Duration = TimeSpan.FromHours(2),
            IsAllDay = false
        };

        return calendarEvent;
    }
    
    
    /// <summary>
    /// Returns a <see cref="CalendarEvent"/> for the time when the doors will open
    /// </summary>
    /// <param name="concert"></param>
    /// <param name="nextEventStart"></param>
    /// <returns>event or null, if the <see cref="Concert.DoorsTime"/> is not set</returns>
    private static CalendarEvent? GetEventForDoorsTime(Concert concert, IDateTime? nextEventStart)
    {
        if (concert.DoorsTime == null) 
            return null;
        
        var title = $"Doors open: Linkin Park in {concert.City}";
        var description = $"Doors open at {concert.Venue} for the Linkin Park concert";

        var date = concert.DoorsTime.Value.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            Summary = title,
            Description = description,
            Location = $"{concert.LocationLong}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = date,
            End = nextEventStart,
            IsAllDay = false
        };

        return calendarEvent;
    }
    
    
    private static CalendarEvent? GetCalendarEventWithTourName(Concert concert)
    {
        if (concert.PostedStartTime == null)
            return null;

        var title = $"{concert.TourName}: {concert.City}";
        var description = $"Concert of the Linkin Park {concert.TourName}";

        CalDateTime? date;
        TimeSpan duration;
        if (concert.MainStageTime != null)
        {
            // if available, use main stage time. In that case, decrease the duration of the event
            date = concert.MainStageTime?.ToCalDateTime(concert.TimeZoneId);
            duration = TimeSpan.FromHours(2);
        }
        else
        {
            // only start time from ticket available
            date = concert.PostedStartTime?.ToCalDateTime(concert.TimeZoneId);
            duration = TimeSpan.FromHours(3);
        }
        
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = title,
            Description = description,
            Location = $"{concert.LocationLong}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = date,
            Duration = duration,
            IsAllDay = false
        };

        return calendarEvent;
    }
    
    
    private static CalendarEvent? GetCalendarEventWithoutTourName(Concert concert)
    {
        if (concert.PostedStartTime == null)
            return null;
        
        var title = $"Linkin Park: {concert.Venue}";
        var description = $"Linkin Park Concert at {concert.Venue}\nThis show is not part of a tour.";

        CalDateTime? date;
        TimeSpan duration;
        if (concert.MainStageTime != null)
        {
            // if available, use main stage time. In that case, decrease the duration of the event
            date = concert.MainStageTime?.ToCalDateTime(concert.TimeZoneId);
            duration = TimeSpan.FromHours(2);
        }
        else
        {
            // only start time from ticket available
            date = concert.PostedStartTime?.ToCalDateTime(concert.TimeZoneId);
            duration = TimeSpan.FromHours(3);
        }
        
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = title,
            Description = description,
            Location = $"{concert.LocationMedium}",
            GeographicLocation = concert.GetGeoLocation(),
            Start = date,
            Duration = duration,
            IsAllDay = false
        };

        return calendarEvent;
    }
}