namespace Common.Utils.Cors;

public static class CorsHeaderFactory
{
    /// <summary>
    /// Value for the Access-Control-Allow-Origin header from environment variables
    /// </summary>
    public static string AllowOriginValue => GetAllowOriginValue();
    
    /// <summary>
    /// Value for the Access-Control-Allow-Origin header from environment variables
    /// </summary>
    /// <returns>* if no value was defined</returns>
    private static string GetAllowOriginValue()
    {
        return Environment.GetEnvironmentVariable("API_CORS_ORIGIN") ?? "*";
    }
    
    /// <summary>
    /// Value for the Access-Control-Allow-Methods header
    /// </summary>
    /// <returns>* if no value was defined</returns>
    public static string GetAllowMethodsHeaderValue(params HttpMethod[] methods)
    {
        List<HttpMethod> allMethods = [HttpMethod.Options];
        allMethods.AddRange(methods);
        return string.Join(", ", allMethods);
    }
    
    public static KeyValuePair<string, string> AllowOriginHeaderFieldFor(string origin)
        => new("Access-Control-Allow-Origin", origin);
    
    public static KeyValuePair<string, string> AllowMethodsHeaderFieldFor(params HttpMethod[] methods)
        => new("Access-Control-Allow-Methods", GetAllowMethodsHeaderValue(methods));
    
    public static KeyValuePair<string, string> AllowMethodsHeaderFieldFor(string methods)
        => new("Access-Control-Allow-Methods", methods);
}