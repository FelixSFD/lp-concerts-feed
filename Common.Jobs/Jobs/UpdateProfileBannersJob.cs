using Microsoft.Extensions.Logging;
using Quartz;
using Service.SharePics;
using Service.Tours;
using Service.Users;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = "UpdateProfileBanners", Group = "LPshows")]
[CronTrigger("0 * * * * ?")]
public sealed class UpdateProfileBannersJob(UserService userService, UserStatsService userStatsService, SharePicService sharePicService, ILogger<UpdateProfileBannersJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing UpdateProfileBannersJob");
        
        var allUsers = userService.GetAllUsers(cancellationToken);
        await foreach (var user in allUsers)
        {
            var concertStatsTask = userStatsService.GetAttendedConcerts(user.Id, cancellationToken);
            var songStatsTask = userStatsService.GetSongsUserHasSeenLiveAsync(user.Id, cancellationToken);
        
            var concertStats = await concertStatsTask;
            var songStats = await songStatsTask;
        
            var data = new Dictionary<string, object>
            {
                { "username", user.Username },
                { "concerts", concertStats.NumberOfConcerts },
                { "cities", concertStats.NumberOfCities },
                { "countries", concertStats.NumberOfCountries },
                { "songs", songStats.Length },
            };
            var image = await sharePicService.GenerateSharePicAsync("user-stats-general", data, 390, 110, cancellationToken: cancellationToken);
            await File.WriteAllBytesAsync($"test-{user.Id}.png", image, cancellationToken);
        }
    }
}