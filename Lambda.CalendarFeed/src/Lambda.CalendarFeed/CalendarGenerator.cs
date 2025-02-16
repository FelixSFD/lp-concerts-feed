using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using LPCalendar.DataStructure;

namespace Lambda.CalendarFeed;

/// <summary>
/// Class to create a <see cref="Calendar"/> with events
/// </summary>
public class CalendarGenerator : ICalendarGenerator
{
    /// <inheritdoc/>
    public Calendar GenerateCalendarWith(IEnumerable<Concert> concerts, ConcertSubEventCategory categoryFlags = ConcertSubEventCategory.AsOneSingleEvent)
    {
        var calendar = new Calendar();
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin")); // TODO: Get correct timezone
        calendar.Events.AddRange(concerts.ToCalendarEvents(categoryFlags));

        return calendar;
    }
}