using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Quartz;
using Service.SharePics;
using Service.SharePics.DataStructure.Templates;
using Service.SharePics.Storage;
using Service.Tours;
using Service.Users;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = "UpdateProfileBanners", Group = "LPshows")]
[CronTrigger("0 15 4 ? * *", Name = "UpdateProfileBanners-nightly")]
[DisallowConcurrentExecution]
public sealed class UpdateProfileBannersJob(UserService userService, UserStatsService userStatsService, SharePicService sharePicService, ISharePicStore sharePicStore, ILogger<UpdateProfileBannersJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Log.StartJob(logger, nameof(UpdateProfileBannersJob));
        var stopwatch = Stopwatch.StartNew();
        
        var allUsers = userService.GetAllUsers(cancellationToken);
        await foreach (var user in allUsers)
        {
            try
            {
                Log.FetchingStatsFor(logger, user.Username, user.Id);
                var (concertStats, songStats) = await userStatsService.GetConcertAndSongStatsAsync(user.Id, cancellationToken);
                
                Log.FetchedStatsFor(logger, user.Username, user.Id);

                var template = new ProfileBannerTemplate
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Concerts =
                    [
                        .. concertStats.Concerts.Select(c => new ProfileBannerTemplate.Concert
                        {
                            Location = $"{c.Venue.City.Name}, {c.Venue.City.Country.Name}",
                            Date = c.PostedStartTime.Date,
                        })
                    ],
                    Countries = concertStats.NumberOfCountries,
                    Cities = concertStats.NumberOfCities,
                    Songs = songStats.Length,
                };
                var image = await sharePicService.GenerateSharePicAsync(template, 390 * 2, 110 * 2, 2,
                    cancellationToken: cancellationToken);
                
                Log.GeneratedProfileBannerFor(logger, user.Username, user.Id);
                
                var memoryStream = new MemoryStream(image);
                await sharePicStore.StorePermanentAsync(memoryStream, $"profile/{user.Id}/banner.png",
                    cancellationToken);
                
                Log.StoredProfileBannerFor(logger, user.Username, user.Id, image.Length);
            }
            catch (Exception e)
            {
                Log.FailedToGenerateProfileBannerFor(logger, user.Username, user.Id, e);
            }
        }
        
        stopwatch.Stop();
        Log.FinishedJob(logger, nameof(UpdateProfileBannersJob), stopwatch.Elapsed.TotalSeconds, context.NextFireTimeUtc);
    }
}