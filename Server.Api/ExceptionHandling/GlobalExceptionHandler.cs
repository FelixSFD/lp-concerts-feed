using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Service.Tours.Exceptions;

namespace Server.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancelaltionToken)
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
                Detail = GetSafeErrorMessage(exception, httpContext)
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
            });
        }

        // Map the exceptions to HTTP responses
        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),

            NotFoundExceptionBase => (StatusCodes.Status404NotFound, "Resource not found"),

            // AppException appException => ((int)appException.StatusCode, "Application Error"),

            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };


        // Function to map the HTTP Status codes to the Problem Type URL
        private static string GetProblemType(int statusCode) => statusCode switch
        {
            400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
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
            return exception is NotFoundExceptionBase ? exception.Message : null;
        }
    }