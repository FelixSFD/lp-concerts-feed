namespace Database.Setlists.DataObjects;

public static class DataConstants
{
    /// <summary>
    /// Maximum length of Concert IDs
    /// </summary>
    internal const int ConcertIdLength = 63;
    
    /// <summary>
    /// Length of the field that contains the title of songs, mashups or variants.
    /// </summary>
    internal const int TitleFieldLength = 127;
    
    /// <summary>
    /// Length of the Album Title field
    /// </summary>
    internal const int AlbumTitleFieldLength = 31;

    /// <summary>
    /// Length of the ISRC
    /// </summary>
    internal const int IsrcLength = 15;

    /// <summary>
    /// Maximum Length of Apple Music IDs
    /// </summary>
    internal const int AppleMusicIdLength = 31;

    /// <summary>
    /// Maximum length of Linkinpedia URLs
    /// </summary>
    internal const int LinkinpediaUrlLength = 127;
}