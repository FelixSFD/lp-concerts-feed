using Common.Utils.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Service.Setlists.Exceptions;
using Service.Setlists.Importer.Exceptions;
using Service.Tours.Exceptions;

namespace Server.Api.ExceptionHandling;

/// <summary>
/// Handles unhandled exceptions globally to get the <see cref="ProblemDetails"/> object.
/// </summary>
/// <param name="logger"></param>
/// <param name="problemDetailsService"></param>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occured. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        var (statusCode, title) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = GetProblemType(statusCode),
            Instance = httpContext.Request.Path,
            Detail = GetSafeErrorMessage(exception, httpContext),
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        var success = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
        });

        if (!success)
        {
            logger.LogWarning("Failed to process problem details. TraceID: {traceId}", httpContext.TraceIdentifier);
        }

        return success;
    }

    // Map the exceptions to HTTP responses
    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        BadRequestException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        InvalidMashupException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        InvalidEntryOrderException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        InvalidWikiContentException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
        LinkinpediaImportServiceException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),

        NotFoundExceptionBase => (StatusCodes.Status404NotFound, "Resource not found"),
        SetlistNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        SongNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        SongVariantNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        SongMashupNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        AlbumNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        SetlistEntryNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        SetlistActNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        Service.Setlists.Exceptions.ConcertNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),

        // AppException appException => ((int)appException.StatusCode, "Application Error"),

        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };


    // Function to map the HTTP Status codes to the Problem Type URL
    private static string GetProblemType(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
    };

    // Function to get safe error message in production and explicit error message in development
    private static string? GetSafeErrorMessage(Exception exception, HttpContext context)
    {
        //only expose details in development
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
        if (env.IsDevelopment())
        {
            return exception.Message;
        }

        //In production, only expose message from custom safe exceptions
        return exception is NotFoundExceptionBase
            or SetlistNotFoundException
            or SongNotFoundException
            or SongVariantNotFoundException
            or SongMashupNotFoundException
            or AlbumNotFoundException
            or SetlistEntryNotFoundException
            or SetlistActNotFoundException
            or Service.Setlists.Exceptions.ConcertNotFoundException
            or InvalidMashupException
            or InvalidEntryOrderException
            or InvalidWikiContentException
            or LinkinpediaImportServiceException
            or BadRequestException
            ? exception.Message
            : null;
    }
}