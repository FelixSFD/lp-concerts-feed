using Database.Tours.DataObjects;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using LPCalendar.DataStructure;
using Service.Calendar.DataStructure;

namespace Service.Calendar;

/// <summary>Maps persisted concert data to iCalendar events.</summary>
internal sealed class CalendarEventMapper
{
    private readonly Uri? _siteBaseUri;
    private readonly string? _subscriptionId;

    /// <summary>Creates a mapper with an optional concert site base URI and subscription ID.</summary>
    /// <param name="siteBaseUri">HTTP(S) site base URI, without a query or fragment.</param>
    /// <param name="subscriptionId">Subscription ID used for the management link, if available.</param>
    public CalendarEventMapper(Uri? siteBaseUri, string? subscriptionId)
    {
        if (siteBaseUri is not null &&
            (!siteBaseUri.IsAbsoluteUri || (siteBaseUri.Scheme != Uri.UriSchemeHttp && siteBaseUri.Scheme != Uri.UriSchemeHttps)
             || !string.IsNullOrEmpty(siteBaseUri.Query) || !string.IsNullOrEmpty(siteBaseUri.Fragment)))
            throw new ArgumentException("The concert site base URI must be an absolute HTTP(S) URI without a query or fragment.", nameof(siteBaseUri));

        _siteBaseUri = siteBaseUri is null ? null : new Uri(siteBaseUri.AbsoluteUri.TrimEnd('/') + "/");
        _subscriptionId = subscriptionId;
    }

    /// <summary>Produces the selected detailed events or the legacy full-concert fallback.</summary>
    /// <param name="concert">Concert with venue, type, and tour references loaded.</param>
    /// <param name="settings">Events requested by the subscription.</param>
    /// <returns>One or more calendar events for the concert.</returns>
    public List<CalendarEvent> ToCalendarEvents(ConcertDo concert, CalendarGenerationSettingsBo settings)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(concert.Venue.TimeZone);
        var events = new List<CalendarEvent>(2);
        var stage = settings.IncludeLpStageTime && concert.MainStageTime.HasValue
            ? CreateStageEvent(concert, timeZone)
            : null;

        if (settings.IncludeDoorsTime && concert.DoorsTime.HasValue)
            events.Add(CreateDoorsEvent(concert, timeZone, stage?.Start));
        if (stage is not null)
            events.Add(stage);
        if (events.Count == 0)
            events.Add(CreateFullEvent(concert, timeZone));
        return events;
    }

    private CalendarEvent CreateStageEvent(ConcertDo concert, TimeZoneInfo zone)
    {
        var startUtc = concert.MainStageTime!.Value;
        var calendarEvent = CreateCommonEvent(concert, "stage");
        calendarEvent.Summary = StageTitle(concert);
        calendarEvent.Description = $"Stage time for Linkin Park at {concert.Venue.CurrentName}.\nType of show: {concert.Type.Name}{ManageSubscriptionHint(concert)}";
        calendarEvent.Location = Location(concert, true);
        calendarEvent.Start = ToLocal(startUtc, zone);
        calendarEvent.End = ToLocal(startUtc.AddMinutes(concert.ExpectedSetDurationMinutes), zone);
        return calendarEvent;
    }

    private CalendarEvent CreateDoorsEvent(ConcertDo concert, TimeZoneInfo zone, CalDateTime? nextStart)
    {
        var calendarEvent = CreateCommonEvent(concert, "doors");
        calendarEvent.Summary = $"Doors open: Linkin Park in {concert.Venue.City.Name}";
        calendarEvent.Description = $"Doors open at {concert.Venue.CurrentName} for the Linkin Park concert{ManageSubscriptionHint(concert)}";
        calendarEvent.Location = Location(concert, true);
        calendarEvent.Start = ToLocal(concert.DoorsTime!.Value, zone);
        calendarEvent.End = nextStart;
        return calendarEvent;
    }

    private CalendarEvent CreateFullEvent(ConcertDo concert, TimeZoneInfo zone)
    {
        var calendarEvent = CreateCommonEvent(concert, "concert");
        var startUtc = concert.MainStageTime ?? concert.PostedStartTime;
        calendarEvent.Summary = ConcertTitle(concert);
        calendarEvent.Description = concert.Tour is not null
            ? $"Concert of the Linkin Park {concert.Tour.Name}{ManageSubscriptionHint(concert)}"
            : $"Linkin Park Concert at {concert.Venue.CurrentName}\nThis show is not part of a tour.{ManageSubscriptionHint(concert)}";
        calendarEvent.Location = Location(concert, concert.Tour is not null);
        calendarEvent.Start = ToLocal(startUtc, zone);
        calendarEvent.End = ToLocal(startUtc.AddHours(concert.MainStageTime.HasValue ? 2 : 3), zone);
        return calendarEvent;
    }

    private CalendarEvent CreateCommonEvent(ConcertDo concert, string eventType)
    {
        var venue = concert.Venue;
        return new CalendarEvent
        {
            Uid = $"{Uri.EscapeDataString(concert.Id)}-{eventType}@lpshows.live",
            Status = concert.Status == ConcertDo.ConcertStatus.Cancelled
                ? EventStatus.Cancelled
                : concert.MainStageTime.HasValue ? EventStatus.Confirmed : EventStatus.Tentative,
            GeographicLocation = venue.Latitude != 0 || venue.Longitude != 0
                ? new GeographicLocation { Latitude = (double)venue.Latitude, Longitude = (double)venue.Longitude }
                : null,
            LastModified = concert.UpdatedAt.HasValue ? new CalDateTime(concert.UpdatedAt.Value.UtcDateTime) : null,
            Url = _siteBaseUri is null ? null : new Uri(_siteBaseUri, $"concert/{Uri.EscapeDataString(concert.Id)}")
        };
    }

    private static CalDateTime ToLocal(DateTime utc, TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);
        // A local TZID value cannot distinguish the two instants in a repeated DST hour.
        if (zone.IsAmbiguousTime(local))
            return new CalDateTime(DateTime.SpecifyKind(utc, DateTimeKind.Utc));
        return new CalDateTime(local, zone.Id);
    }

    private static string Location(ConcertDo concert, bool includeState) =>
        LocationStringBuilder.GetLocationString(concert.Venue.CurrentName, concert.Venue.City.Name,
            includeState ? concert.Venue.State?.Name : null, concert.Venue.Country.Name);

    private static string ConcertTitle(ConcertDo concert) =>
        !string.IsNullOrEmpty(concert.CustomTitle) ? concert.CustomTitle :
        concert.Type.Name == "Linkin Park Show" && !string.IsNullOrEmpty(concert.Tour?.Name)
            ? $"{concert.Tour.Name}: {concert.Venue.City.Name}"
            : $"Linkin Park: {concert.Venue.CurrentName}";

    private static string StageTitle(ConcertDo concert) =>
        !string.IsNullOrEmpty(concert.CustomTitle) ? concert.CustomTitle :
        concert.Type.Name == "Linkin Park Show" && !string.IsNullOrEmpty(concert.Tour?.Name)
            ? $"{concert.Tour.Name}: {concert.Venue.City.Name} (Stage Time)"
            : $"Linkin Park: {concert.Venue.City.Name} (Stage Time)";

    private string AppHint(ConcertDo concert) =>
        _siteBaseUri is null ? string.Empty :
            $"\n\nTry our new FREE app: {new Uri(_siteBaseUri, $"app?mtm_kwd={Uri.EscapeDataString(concert.Id)}&mtm_campaign=ical-feed")}";
    
    private string ManageSubscriptionHint(ConcertDo concert) =>
        _siteBaseUri is null || _subscriptionId is null ? string.Empty :
            $"\n\nChange the settings of this calendar: {new Uri(_siteBaseUri, $"calendar-subscriptions/{Uri.EscapeDataString(_subscriptionId)}?mtm_kwd={Uri.EscapeDataString(concert.Id)}&mtm_campaign=ical-feed").AbsoluteUri}";
}
