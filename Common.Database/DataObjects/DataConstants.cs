namespace Common.Database.DataObjects;

public static class DataConstants
{
    /// <summary>
    /// Maximum length of Concert IDs
    /// </summary>
    public const int ConcertIdLength = 63;
    
    /// <summary>
    /// Length of the field that contains the title of songs, mashups or variants.
    /// </summary>
    public const int TitleFieldLength = 127;
    
    /// <summary>
    /// Length of the Album Title field
    /// </summary>
    public const int AlbumTitleFieldLength = 31;

    /// <summary>
    /// Length of the ISRC
    /// </summary>
    public const int IsrcLength = 15;

    /// <summary>
    /// Maximum Length of Apple Music IDs
    /// </summary>
    public const int AppleMusicIdLength = 31;

    /// <summary>
    /// Maximum length of Linkinpedia URLs
    /// </summary>
    public const int LinkinpediaUrlLength = 127;
    
    /// <summary>
    /// Maximum length of country names
    /// </summary>
    public const int CountryNameLength = 63;
    
    /// <summary>
    /// Maximum length of country codes
    /// </summary>
    public const int CountryCodeLength = 3;
    
    /// <summary>
    /// Maximum length of state codes (like "CA")
    /// </summary>
    public const int StateCodeLength = 3;
    
    /// <summary>
    /// Maximum length of city names
    /// </summary>
    public const int CityNameLength = 63;
    
    /// <summary>
    /// Maximum length of venue names
    /// </summary>
    public const int VenueNameLength = 127;
    
    /// <summary>
    /// Maximum length of time zone IDs
    /// </summary>
    public const int TimeZoneIdLength = 31;
}