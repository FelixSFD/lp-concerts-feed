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
}