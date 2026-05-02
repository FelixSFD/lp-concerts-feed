namespace Common.Utils.Cache;

[Flags]
public enum CacheFlags
{
    None = 0,
    Public = 1,
    Private = 2,
    NoStore = 4,
    MustRevalidate = 8,
    NoCache = 16,
    // Use this flag if you want to pass the timeout as s-max-age instead of max-age.
    // This will set max-age=0, which makes sure the client revalidates the response, but the server does still cache
    UseMaxAgeForServerOnly = 32,
}