using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.APIGatewayEvents;
using Common.Utils.Cache;
using Common.Utils.Cors;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;

namespace Lambda.Common.ApiGateway;

/// <summary>
/// Helper class to build API gateway responses.
/// </summary>
public static class ApiGatewayResponseHelper
{
    #region HTTP 200 - OK

    /// <summary>
    /// Returns <paramref name="value"/> as JSON in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="value">Value to return in the response</param>
    /// <param name="jsonTypeInfo">Info for the JSON parser</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <typeparam name="TValue">Type of the object that will be serialized</typeparam>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse Ok<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo, params HttpMethod[] corsMethods)
    {
        return Ok(value, jsonTypeInfo, null, CorsHeaderFactory.GetAllowMethodsHeaderValue(corsMethods), CorsHeaderFactory.AllowOriginValue);
    }
    
    /// <summary>
    /// Returns <paramref name="value"/> as JSON in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="value">Value to return in the response</param>
    /// <param name="jsonTypeInfo">Info for the JSON parser</param>
    /// <param name="cacheControl">Value for the cache-control header. If null, <see cref="CacheControlHeaderConfig.Default"/> will be used</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <typeparam name="TValue">Type of the object that will be serialized</typeparam>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse Ok<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CacheControlHeaderValue? cacheControl, params HttpMethod[] corsMethods)
    {
        return Ok(value, jsonTypeInfo, cacheControl, CorsHeaderFactory.GetAllowMethodsHeaderValue(corsMethods), CorsHeaderFactory.AllowOriginValue);
    }
    
    /// <summary>
    /// Returns <paramref name="value"/> as JSON in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="value">Value to return in the response</param>
    /// <param name="jsonTypeInfo">Info for the JSON parser</param>
    /// <param name="cacheControl">Value of the Cache-Control header. You can generate it using the <see cref="CacheControlHeaderFactory"/>. Default: <see cref="CacheExpiration.Default"/> and public</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <param name="corsOrigin">allowed CORS Origin</param>
    /// <typeparam name="TValue">Type of the object that will be serialized</typeparam>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse Ok<TValue>(TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CacheControlHeaderValue? cacheControl, string corsMethods, string corsOrigin)
    {
        return BuildResponseWithBody(HttpStatusCode.OK, value, jsonTypeInfo, cacheControl ?? CacheControlHeaderConfig.Default, corsMethods, corsOrigin);
    }

    #endregion
    
    #region HTTP 204 - No Content
    
    /// <summary>
    /// Returns an HTTP 204 in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse NoContent(params HttpMethod[] corsMethods)
    {
        return NoContent(null, CorsHeaderFactory.GetAllowMethodsHeaderValue(corsMethods), CorsHeaderFactory.AllowOriginValue);
    }
    
    /// <summary>
    /// Returns an HTTP 204 in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="cacheControl">Value for the cache-control header. If null, <see cref="CacheControlHeaderConfig.Default"/> will be used</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse NoContent(CacheControlHeaderValue? cacheControl, params HttpMethod[] corsMethods)
    {
        return NoContent(cacheControl, CorsHeaderFactory.GetAllowMethodsHeaderValue(corsMethods), CorsHeaderFactory.AllowOriginValue);
    }
    
    /// <summary>
    /// Returns an HTTP 204 in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="cacheControl">Value of the Cache-Control header. You can generate it using the <see cref="CacheControlHeaderFactory"/>. Default: <see cref="CacheExpiration.Default"/> and public</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <param name="corsOrigin">allowed CORS Origin</param>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse NoContent(CacheControlHeaderValue? cacheControl, string corsMethods, string corsOrigin)
    {
        return BuildResponse(HttpStatusCode.NoContent, cacheControl ?? CacheControlHeaderConfig.Default, corsMethods, corsOrigin);
    }

    #endregion
    
    #region HTTP 404 - Not Found

    /// <summary>
    /// Returns the error message as JSON in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="errorMessage">Error message to return to the client</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse NotFound(string errorMessage, params HttpMethod[] corsMethods)
    {
        return NotFound(errorMessage, null, CorsHeaderFactory.GetAllowMethodsHeaderValue(corsMethods), CorsHeaderFactory.AllowOriginValue);
    }
    
    /// <summary>
    /// Returns the error message as JSON in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="errorMessage">Error message to return to the client</param>
    /// <param name="cacheControl">Value for the cache-control header. If null, <see cref="CacheControlHeaderConfig.Default"/> will be used</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse NotFound(string errorMessage, CacheControlHeaderValue? cacheControl, params HttpMethod[] corsMethods)
    {
        return NotFound(errorMessage, cacheControl, CorsHeaderFactory.GetAllowMethodsHeaderValue(corsMethods), CorsHeaderFactory.AllowOriginValue);
    }
    
    /// <summary>
    /// Returns the error message as JSON in the <see cref="APIGatewayProxyResponse"/>
    /// </summary>
    /// <param name="errorMessage">Error message to return to the client</param>
    /// <param name="cacheControl">Value of the Cache-Control header. You can generate it using the <see cref="CacheControlHeaderFactory"/>. Default: <see cref="CacheExpiration.Default"/> and public</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <param name="corsOrigin">allowed CORS Origin</param>
    /// <returns>response for the API gateway</returns>
    public static APIGatewayProxyResponse NotFound(string errorMessage, CacheControlHeaderValue? cacheControl, string corsMethods, string corsOrigin)
    {
        var errorResponseDto = new ErrorResponse
        {
            Message = errorMessage,
        };
        return BuildResponseWithBody(HttpStatusCode.NotFound, errorResponseDto, DataStructureJsonContext.Default.ErrorResponse, cacheControl ?? CacheControlHeaderConfig.Default, corsMethods, corsOrigin);
    }

    #endregion
    
    /// <summary>
    /// Builds the <see cref="APIGatewayProxyResponse"/> for a given status code without body
    /// </summary>
    /// <param name="httpStatusCode">HTTP status code</param>
    /// <param name="cacheControl">Value of the Cache-Control header. You can generate it using the <see cref="CacheControlHeaderFactory"/>. Default: <see cref="CacheExpiration.Default"/> and public</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <param name="corsOrigin">allowed CORS Origin</param>
    /// <returns></returns>
    private static APIGatewayProxyResponse BuildResponse(HttpStatusCode httpStatusCode, CacheControlHeaderValue cacheControl, string corsMethods, string corsOrigin)
    {
        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)httpStatusCode,
            Headers = new Dictionary<string, string>(),
        };
        
        response.Headers.Add(CorsHeaderFactory.AllowOriginHeaderFieldFor(corsOrigin));
        response.Headers.Add(CorsHeaderFactory.AllowMethodsHeaderFieldFor(corsMethods));
        response.Headers.Add(CacheControlHeaderFactory.CacheHeaderFieldFor(cacheControl.ToString()));
        
        return response;
    }
    
    /// <summary>
    /// Builds the <see cref="APIGatewayProxyResponse"/> for a given status code and return value
    /// </summary>
    /// <param name="httpStatusCode">HTTP status code</param>
    /// <param name="value">Value to return in the response</param>
    /// <param name="jsonTypeInfo">Info for the JSON parser</param>
    /// <param name="cacheControl">Value of the Cache-Control header. You can generate it using the <see cref="CacheControlHeaderFactory"/>. Default: <see cref="CacheExpiration.Default"/> and public</param>
    /// <param name="corsMethods">allowed CORS methods (OPTIONS will be included by default)</param>
    /// <param name="corsOrigin">allowed CORS Origin</param>
    /// <typeparam name="TValue">Type of the object that will be serialized</typeparam>
    /// <returns></returns>
    private static APIGatewayProxyResponse BuildResponseWithBody<TValue>(HttpStatusCode httpStatusCode, TValue value, JsonTypeInfo<TValue> jsonTypeInfo, CacheControlHeaderValue cacheControl, string corsMethods, string corsOrigin)
    {
        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)httpStatusCode,
            Body = JsonSerializer.Serialize(value, jsonTypeInfo),
            Headers = new Dictionary<string, string>(),
        };
        
        response.Headers.Add(CorsHeaderFactory.AllowOriginHeaderFieldFor(corsOrigin));
        response.Headers.Add(CorsHeaderFactory.AllowMethodsHeaderFieldFor(corsMethods));
        response.Headers.Add(CacheControlHeaderFactory.CacheHeaderFieldFor(cacheControl.ToString()));
        
        return response;
    }
}