using Ical.Net;
using LPCalendar.DataStructure;

namespace Lambda.CalendarFeed;

/// <summary>
/// Interface to provide functions that generate <see cref="Calendar"/>s
/// </summary>
public interface ICalendarGenerator
{
    /// <summary>
    /// Generates a <see cref="Calendar"/> from a list of <see cref="Concert"/>
    /// </summary>
    /// <param name="concerts"></param>
    /// <param name="categoryFlags">options, which events to create for each concert</param>
    /// <returns></returns>
    Calendar GenerateCalendarWith(IEnumerable<Concert> concerts, ConcertSubEventCategory categoryFlags = ConcertSubEventCategory.AsOneSingleEvent);
}