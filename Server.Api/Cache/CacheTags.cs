namespace Server.Api.Cache;

public static class CacheTags
{
    /// <summary>
    /// Tag for all concerts
    /// </summary>
    public const string ConcertsAll = "concerts-all";

    /// <summary>
    /// Tag for all concert bookmarks
    /// </summary>
    public const string ConcertBookmarksAll = "concert-bookmarks-all";

    /// <summary>
    /// Tag for a single concert
    /// </summary>
    public const string Concert = "concert";

    /// <summary>
    /// Tag for all countries
    /// </summary>
    public const string CountriesAll = "countries-all";

    /// <summary>
    /// Tag for all cities
    /// </summary>
    public const string CitiesAll = "cities-all";

    /// <summary>
    /// Tag for all states
    /// </summary>
    public const string StatesAll = "states-all";

    /// <summary>
    /// Tag for all venues
    /// </summary>
    public const string VenuesAll = "venues-all";

    /// <summary>
    /// Tag for all users
    /// </summary>
    public const string UsersAll = "users-all";

    /// <summary>
    /// Tag for all setlists
    /// </summary>
    public const string SetlistsAll = "setlists-all";

    /// <summary>
    /// Tag for all albums
    /// </summary>
    public const string AlbumsAll = "albums-all";

    /// <summary>
    /// Tag for all songs
    /// </summary>
    public const string SongsAll = "songs-all";

    /// <summary>
    /// Tag for all mashups
    /// </summary>
    public const string MashupsAll = "mashups-all";
    
    /// <summary>
    /// Cache-Tag for the user stats
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <returns></returns>
    public static string UserStats(string userId) => $"user-{userId}-stats";
}