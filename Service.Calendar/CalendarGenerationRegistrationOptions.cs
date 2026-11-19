namespace Service.Calendar;

/// <summary>
/// Options for registering calendar generation with the frontend base URL.
/// </summary>
public class CalendarGenerationRegistrationOptions
{
    /// <summary>
    /// Base-URL for the frontend. Will be used to generate links in the calendar events. Must be an absolute HTTP(S) URL without a query or fragment.
    /// </summary>
    public required string FrontendBaseUrl { get; set; }
}