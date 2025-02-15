using Ical.Net;
using Ical.Net.CalendarComponents;
using LPCalendar.DataStructure;

namespace Lambda.CalendarFeed;

public class CalendarGenerator
{
    /// <summary>
    /// Returns one or more <see cref="CalendarEvent"/>s for a <see cref="Concert"/>.
    /// This depends on the settings whether a detailed calendar is requested.
    /// </summary>
    /// <param name="concert">Concert to create the events for</param>
    /// <param name="categoryFlags">Flags to specify which sub-events to return for the concert</param>
    /// <returns>one or more events</returns>
    public IEnumerable<CalendarEvent?> ToCalendarEvents(Concert concert, ConcertSubEventCategory categoryFlags = ConcertSubEventCategory.AsOneSingleEvent)
    {
        if (categoryFlags.HasFlag(ConcertSubEventCategory.AsOneSingleEvent))
        {
            yield return GetFullEventFor(concert);
        }
        else
        {
            // More detailed calendar was requested -> Create events for every part of the Concert (if available)

            CalendarEvent? lpStageTimeEvent = null;
            if (categoryFlags.HasFlag(ConcertSubEventCategory.LinkinPark))
            {
                lpStageTimeEvent = GetEventForLinkinParkStageTime(concert);
            }

            yield return lpStageTimeEvent;
        }
    }
    
    
    /// <summary>
    /// Returns the <see cref="CalendarEvent"/> for a <see cref="Concert"/> as one entry for the full show
    /// </summary>
    /// <param name="concert">Concert to generate the event for</param>
    /// <returns>Calendar Event</returns>
    public CalendarEvent? GetFullEventFor(Concert concert)
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
            
        // TODO: make location string
            
        var date = concert.MainStageTime.Value.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            Summary = title,
            Description = description,
            //Location = $"{concert.Venue}, {concert.City}{stateString}, {concert.Country}",
            Start = date,
            Duration = TimeSpan.FromHours(2),
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
        var stateString = concert.State != null ? $", {concert.State}" : "";
        
        var date = concert.PostedStartTime?.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = title,
            Description = description,
            Location = $"{concert.Venue}, {concert.City}{stateString}, {concert.Country}",
            Start = date,
            Duration = TimeSpan.FromHours(3),
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
        var stateString = concert.State != null ? $", {concert.State}" : "";

        var date = concert.PostedStartTime?.ToCalDateTime(concert.TimeZoneId);
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = title,
            Description = description,
            Location = $"{concert.City}{stateString}, {concert.Country}",
            Start = date,
            Duration = TimeSpan.FromHours(3),
            IsAllDay = false
        };

        return calendarEvent;
    }
}