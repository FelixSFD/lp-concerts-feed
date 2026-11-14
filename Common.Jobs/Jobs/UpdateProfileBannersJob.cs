using Microsoft.Extensions.Logging;
using Quartz;
using Service.SharePics;
using Service.SharePics.Storage;
using Service.Tours;
using Service.Users;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = "UpdateProfileBanners", Group = "LPshows")]
[CronTrigger("0 * * * * ?")]
public sealed class UpdateProfileBannersJob(UserService userService, UserStatsService userStatsService, SharePicService sharePicService, ISharePicStore sharePicStore, ILogger<UpdateProfileBannersJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing UpdateProfileBannersJob");
        
        var allUsers = userService.GetAllUsers(cancellationToken);
        await foreach (var user in allUsers)
        {
            try
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
                var image = await sharePicService.GenerateSharePicAsync("user-stats-general", data, 390, 110,
                    cancellationToken: cancellationToken);
                var memoryStream = new MemoryStream(image);
                await sharePicStore.StorePermanentAsync(memoryStream, $"profile/{user.Id}/banner.png",
                    cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to generate profile banner for user: {username} (ID: {userId})", user.Username, user.Id);
            }
        }
    }
}