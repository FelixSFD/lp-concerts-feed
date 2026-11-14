using Microsoft.Extensions.Logging;
using Quartz;
using Service.SharePics;
using Service.SharePics.DataStructure.Templates;
using Service.SharePics.Storage;
using Service.Tours;
using Service.Users;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = "UpdateProfileBanners", Group = "LPshows")]
[CronTrigger("0 15 6 ? * *")]
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

                var template = new ProfileBannerTemplate
                {
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
                var image = await sharePicService.GenerateSharePicAsync(template, 390, 110,
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