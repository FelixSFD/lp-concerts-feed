namespace Service.Tours.Importer.DataStructure;

/// <summary>
/// Information extracted from a Linkinpedia Tourdate template
/// </summary>
public class WikiTourdateEntry
{
    /// <summary>
    /// Type of the show, e.g. "concert", "rehearsal", "festival"
    /// </summary>
    public string? ShowType { get; set; }

    /// <summary>
    /// Artist name, e.g. "Linkin Park"
    /// </summary>
    public string? Artist { get; set; }
    
    /// <summary>
    /// Mainly used for festivals like "Rock am Ring"
    /// </summary>
    public string? Event { get; set; }

    /// <summary>
    /// Date of the previous show
    /// </summary>
    public string? LastShow { get; set; }

    /// <summary>
    /// Date of the next show
    /// </summary>
    public string? NextShow { get; set; }

    /// <summary>
    /// Year of the concert
    /// </summary>
    public uint? Year { get; set; }

    /// <summary>
    /// Month of the concert (name or number)
    /// </summary>
    public string? Month { get; set; }

    /// <summary>
    /// Day of the concert
    /// </summary>
    public uint? Day { get; set; }

    /// <summary>
    /// Full date of the concert parsed from Year, Month, Day
    /// </summary>
    public DateOnly? Date { get; set; }

    /// <summary>
    /// Country where the concert took place
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// City where the concert took place (may also include state)
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Venue name
    /// </summary>
    public string? Venue { get; set; }

    /// <summary>
    /// Venue identifier on Linkinpedia
    /// </summary>
    public string? VenueId { get; set; }

    /// <summary>
    /// Type of venue, e.g. "Amphitheatre", "Stadium", "Arena"
    /// </summary>
    public string? VenueType { get; set; }

    /// <summary>
    /// Website URL of the venue
    /// </summary>
    public string? VenueWebsite { get; set; }

    /// <summary>
    /// Name of the tour
    /// </summary>
    public string? Tour { get; set; }
    
    /// <summary>
    /// Name of the tour leg
    /// </summary>
    public string? TourLeg { get; set; }

    /// <summary>
    /// Stage name, e.g. "Main Stage"
    /// </summary>
    public string? Stage { get; set; }

    /// <summary>
    /// Other artists performing at the concert
    /// </summary>
    public string? OtherArtists { get; set; }

    /// <summary>
    /// First support act
    /// </summary>
    public string? Support { get; set; }

    /// <summary>
    /// Second support act
    /// </summary>
    public string? Support2 { get; set; }

    /// <summary>
    /// List of all support acts extracted from Support, Support2, etc.
    /// </summary>
    public string[] SupportArtists { get; set; } = [];

    /// <summary>
    /// Setlist code or identifier, e.g. "B6"
    /// </summary>
    public string? Setlist { get; set; }

    /// <summary>
    /// All raw key-value pairs parsed from the Tourdate template
    /// </summary>
    public Dictionary<string, string> RawProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
