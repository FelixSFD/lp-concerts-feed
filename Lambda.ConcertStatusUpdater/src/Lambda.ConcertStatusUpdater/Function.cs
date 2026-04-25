using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Amazon.Lambda.Core;
using Database.Concerts;
using Database.Concerts.Models;
using LPCalendar.DataStructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ConcertStatusUpdater;

using DateRange = (DateTimeOffset? from, DateTimeOffset? to);

public class Function(IConcertRepository concertRepository)
{
    private static readonly TimeSpan StartTimeRange = TimeSpan.FromMinutes(11);
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task FunctionHandler(ScheduledEvent evt, ILambdaContext context)
    {
        var eventTimeUtc = new DateTimeOffset(evt.Time, TimeSpan.Zero);
        var relevantRange = (eventTimeUtc.AddHours(-3), eventTimeUtc.AddHours(7));
        
        await foreach (var concert in GetRelevantConcerts(relevantRange))
        {
            await UpdateConcertStatus(concert, eventTimeUtc, context.Logger);
        }
    }
    
    private IAsyncEnumerable<ConcertModel> GetRelevantConcerts(DateRange inDateRange)
    { 
        var planned = GetPlannedConcerts(inDateRange);
        var running = GetRunningConcerts(inDateRange);
        return planned.Concat(running);
    }
    
    private IAsyncEnumerable<ConcertModel> GetPlannedConcerts(DateRange inDateRange)
    { 
        return concertRepository.GetConcertsByStatusAsync(nameof(ConcertDto.ConcertStatusValue.Planned), inDateRange);
    }
    
    private IAsyncEnumerable<ConcertModel> GetRunningConcerts(DateRange inDateRange)
    { 
        return concertRepository.GetConcertsByStatusAsync(nameof(ConcertDto.ConcertStatusValue.Running), inDateRange);
    }

    private async Task UpdateConcertStatus(ConcertModel concertModel, DateTimeOffset atTime, ILambdaLogger logger)
    {
        var currentStatus = concertModel.ConcertStatus;
        var newStatus = RecommendedStatusFor(concertModel, atTime).ToString();
        if (currentStatus != newStatus)
        {
            logger.LogInformation("Concert '{id}': Updating concert status from '{oldStatus}' to '{newStatus}'", concertModel.Id, currentStatus, newStatus);
            concertModel.ConcertStatus = newStatus;
            await concertRepository.SaveAsync(concertModel);
        }
        else
        {
            logger.LogDebug("Concert '{id}': Status '{status}' is already correct.", concertModel.Id, currentStatus);
        }
    }


    private static ConcertDto.ConcertStatusValue RecommendedStatusFor(ConcertModel concertModel, DateTimeOffset atTime)
    {
        var isRunning = IsConcertRunning(concertModel, atTime);
        if (isRunning)
        {
            return ConcertDto.ConcertStatusValue.Running;
        }

        return concertModel.IsPast ? ConcertDto.ConcertStatusValue.Past : ConcertDto.ConcertStatusValue.Planned;
    }
    
    
    private static bool IsConcertRunning(ConcertModel concertModel, DateTimeOffset atTime)
    {
        // check if running status is even possible. It's only useful to have this status if we know exact times
        var canBeRunning = concertModel is { MainStageTime: not null, ExpectedSetDuration: > 10 };
        if (!canBeRunning)
            return false;

        var startTime = concertModel.MainStageTime!.Value;
        var endTime = startTime.AddMinutes(concertModel.ExpectedSetDuration ?? 0);
        return startTime.Add(-StartTimeRange) <= atTime && atTime <= endTime.Add(StartTimeRange);
    }
}