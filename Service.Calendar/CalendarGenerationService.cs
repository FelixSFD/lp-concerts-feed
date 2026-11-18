using Database.Tours.Repositories;
using Ical.Net.CalendarComponents;
using Service.Calendar.DataStructure;
using IcalCalendar = Ical.Net.Calendar;

namespace Service.Calendar;

/// <summary>Generates an in-memory concert calendar from the tours database.</summary>
/// <param name="concerts">Repository supplying concerts and their venue, type, and tour references.</param>
/// <param name="concertSiteBaseUri">Optional HTTP(S) base URI used for concert links and app hints.</param>
public sealed class CalendarGenerationService(IConcertRepository concerts, Uri? concertSiteBaseUri = null)
{
    /// <summary>
    /// Generates a calendar for all non-deleted concerts, including past and cancelled shows.
    /// If neither selected detailed event is available, a full-concert event is emitted instead.
    /// </summary>
    /// <param name="settings">The subscription's calendar generation settings.</param>
    /// <param name="cancellationToken">Token used to cancel retrieval and generation.</param>
    /// <returns>The generated calendar, without serializing or storing it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is null.</exception>
    /// <exception cref="ArgumentException">The configured concert site URI is invalid.</exception>
    /// <exception cref="TimeZoneNotFoundException">A concert's venue timezone cannot be resolved.</exception>
    public async Task<IcalCalendar> GenerateCalendarAsync(CalendarGenerationSettingsBo settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();
        var eventMapper = new CalendarEventMapper(concertSiteBaseUri);
        var calendar = new IcalCalendar();
        var earliestByTimeZone = new Dictionary<string, DateTime>(StringComparer.Ordinal);

        await foreach (var concert in concerts.FindAllWithReferencesAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var events = eventMapper.ToCalendarEvents(concert, settings);
            calendar.Events.AddRange(events);
            var earliest = events.Min(e => e.Start!.Value);
            var timeZoneId = concert.Venue.TimeZone;
            if (!earliestByTimeZone.TryGetValue(timeZoneId, out var previous) || earliest < previous)
                earliestByTimeZone[timeZoneId] = earliest;
        }

        foreach (var (timeZoneId, earliest) in earliestByTimeZone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var earliestSupported = earliest < DateTime.UtcNow ? earliest : DateTime.UtcNow;
            calendar.AddTimeZone(VTimeZone.FromDateTimeZone(timeZoneId, earliestSupported, includeHistoricalData: true));
        }

        return calendar;
    }
}
