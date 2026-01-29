using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Common.DynamoDb;
using Database.ConcertBookmarks;
using Database.Concerts;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ListConcerts;

using DateRange = (DateTimeOffset? from, DateTimeOffset? to);

public class Function(ILambdaContext context, IConcertRepository concertRepository)
{
    private readonly IConcertRepository _concertRepository = concertRepository;
    private readonly IConcertBookmarkRepository _concertBookmarkRepository = DynamoDbConcertBookmarkRepository.CreateDefault(context.Logger);


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Path: {path}", request.Path);
        //context.Logger.LogInformation($"Request: {JsonSerializer.Serialize(request, ApiGatewayJsonContext.Default.APIGatewayProxyRequest)}");
        
        if (request.QueryStringParameters != null)
        {
            var idParamFound = request.QueryStringParameters.TryGetValue("id", out var searchId);
            if (idParamFound && searchId != null)
            {
                // Find one concert
                return await ReturnSingleConcert(searchId);
            }
            
            // check date range
            var dateFromFound = request.QueryStringParameters.TryGetValue("date_from", out var dateFromStr);
            var dateToFound = request.QueryStringParameters.TryGetValue("date_to", out var dateToStr);

            var onlyFuture = !dateFromFound && !dateToFound;
            
            var filterTourParameterFound = request.QueryStringParameters.TryGetValue("tour", out var filterTourStr);
            if (filterTourParameterFound || dateFromFound  || dateToFound)
            {
                var dateRangeFilter = GetDateRangeFrom(dateFromStr, dateToStr, context.Logger);

                return await ReturnFilteredConcertList(context, filterTourStr, dateRangeFilter);
            }

            // no filters were used. Return all concerts
            context.Logger.LogDebug("Content of only_future variable: {value}", onlyFuture);
            return await ReturnAllConcerts(context, onlyFuture);
        }
        
        if (request.Path == "/concerts/next")
        {
            context.Logger.LogInformation("Requested next concert");
            return await ReturnNextConcert(context);
        }

        
        if (request.Path is "/concerts/attending" or "/concerts/bookmarked")
        {
            context.Logger.LogDebug("Requested attending or bookmarked concerts for a user.");
            var currentUserId = request.GetUserId();
            context.Logger.LogInformation("Requested bookmarked concerts for user '{currentUserId}'", currentUserId);

            if (currentUserId == null)
            {
                return UnauthorizedResponseHelper.GetResponse("OPTIONS, GET");
            }

            var status = request.Path is "/concerts/attending"
                ? ConcertBookmark.BookmarkStatus.Attending
                : ConcertBookmark.BookmarkStatus.Bookmarked;
            return await ReturnBookmarkedConcerts(status, 5, currentUserId, context);
        }
        
        if (request.PathParameters != null && request.PathParameters.TryGetValue("id", out var idParameter))
        {
            context.Logger.LogInformation("Requested ID: {id}", idParameter);
            if (idParameter == "next")
            {
                return await ReturnNextConcert(context);
            }

            return await ReturnSingleConcert(idParameter);
        }
        
        // List all concerts
        context.Logger.LogDebug("Fallback to return all FUTURE concerts");
        return await ReturnAllConcerts(context);
    }


    private async Task<APIGatewayProxyResponse> ReturnFilteredConcertList(ILambdaContext context, string? filterTour = null, DateRange? dateRange = null)
    {
        var concerts = await _concertRepository.GetConcertsAsync(filterTour, dateRange).ToListAsync();
        
        var concertsJson = JsonSerializer.Serialize(concerts, DataStructureJsonContext.Default.ListConcert);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = concertsJson,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }


    private async Task<APIGatewayProxyResponse> ReturnNextConcert(ILambdaContext context)
    {
        var next = await _concertRepository.GetNextAsync();
        if (next == null)
        {
            context.Logger.LogInformation("No upcoming concert found.");
            var error = new ErrorResponse
            {
                Message = "No upcoming concerts found."
            };
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body = JsonSerializer.Serialize(error, DataStructureJsonContext.Default.ErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }

        context.Logger.LogDebug("Returning Concert with ID: {id}", next.Id);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(next, DataStructureJsonContext.Default.Concert),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }


    private async Task<APIGatewayProxyResponse> ReturnSingleConcert(string id)
    {
        var concert = await GetConcertById(id);
        if (concert == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 404,
                Body = "{\"message\": \"Concert not found.\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        
        var concertJson = JsonSerializer.Serialize(concert, DataStructureJsonContext.Default.Concert);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = concertJson,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }
    
    
    private async Task<APIGatewayProxyResponse> ReturnAllConcerts(ILambdaContext context, bool onlyFuture = true)
    {
        var searchStartDate = onlyFuture ? DateTimeOffset.Now : DateTimeOffset.MinValue;

        var concerts = await _concertRepository.GetConcertsAsync(searchStartDate).ToListAsync();
        
        var concertJson = JsonSerializer.Serialize(concerts, DataStructureJsonContext.Default.ListConcert);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = concertJson,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };
    }
    
    
    private async Task<APIGatewayProxyResponse> ReturnBookmarkedConcerts(ConcertBookmark.BookmarkStatus status, int maxResults, string currentUserId, ILambdaContext context)
    {
        var searchStartDate = DateTimeOffset.UtcNow.AddHours(-4);
        var bookmarks = _concertBookmarkRepository.GetForUserAsync(currentUserId, status);
        var sortedAndFiltered = bookmarks
            .SelectAwait(async cb => await GetConcertForBookmarkAndMerge(cb))
            .NotNull()
            .Where(c => c.PostedStartTime >= searchStartDate)
            .OrderBy(ec => ec.PostedStartTime)
            .Take(maxResults);
        
        var json = JsonSerializer.Serialize(await sortedAndFiltered.ToListAsync(), DataStructureJsonContext.Default.ListConcertWithBookmarkStatusResponse);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = json,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }


    private async Task<ConcertWithBookmarkStatusResponse?> GetConcertForBookmarkAndMerge(
        ConcertBookmark bookmark)
    {
        var concert = await GetConcertById(bookmark.ConcertId);
        return concert != null ? ConcertWithBookmarkStatusResponse.FromConcert(concert, bookmark) : null;
    }


    /// <summary>
    /// Returns a concert by its ID
    /// </summary>
    /// <param name="id">ID</param>
    /// <returns>Concert</returns>
    private async Task<Concert?> GetConcertById(string id)
    {
        return await _concertRepository.GetByIdAsync(id);
    }


    /// <summary>
    /// Returns a date range as tuple in UTC. The inputs are allowed to be any timezone as long as the offset is specified
    /// </summary>
    /// <param name="fromStr">ISO String</param>
    /// <param name="toStr">ISO String</param>
    /// <param name="logger">Logger</param>
    /// <returns>tuple of DateTimeOffsets</returns>
    private static DateRange GetDateRangeFrom(string? fromStr, string? toStr, ILambdaLogger logger)
    {
        logger.LogDebug("GetDateRangeFrom: {from}, {to}", fromStr, toStr);
        var fromParsed = DateTimeOffset.TryParse(fromStr ?? "", out var dateFrom);
        var toParsed = DateTimeOffset.TryParse(toStr ?? "", out var dateTo);
        
        // make sure to use UTC time
        var dateRange = new DateRange(fromParsed ? dateFrom.ToOffset(TimeSpan.Zero) : null, toParsed ? dateTo.ToOffset(TimeSpan.Zero) : null);
        logger.LogDebug("Parsed from: {from}: Parsed to: {to}", fromParsed, toParsed);
        return dateRange;
    }
}