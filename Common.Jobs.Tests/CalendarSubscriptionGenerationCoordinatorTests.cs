using Amazon.S3;
using Amazon.S3.Model;
using Database.Tours.Repositories;
using Database.Users.DataObjects;
using Database.Users.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Service.Calendar;

namespace Common.Jobs.Tests;

public class CalendarSubscriptionGenerationCoordinatorTests
{
    [Fact]
    public void Constructor_Throws_WhenCalendarSubscriptionsBucketNameIsMissing()
    {
        var concerts = Substitute.For<IConcertRepository>();
        var config = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() => new CalendarSubscriptionGenerationCoordinator(
            Substitute.For<ICalendarSubscriptionRepository>(), concerts, new CalendarGenerationService(concerts),
            Substitute.For<IAmazonS3>(), config, NullLogger<CalendarSubscriptionGenerationCoordinator>.Instance));

        Assert.Equal("CalendarSubscriptions:BucketName is missing!", exception.Message);
    }

    [Fact]
    public async Task GenerateSingleSubscriptionAsync_GeneratesAndUploadsCalendar_WhenSubscriptionIsOutdated()
    {
        var subscriptions = Substitute.For<ICalendarSubscriptionRepository>();
        var concerts = Substitute.For<IConcertRepository>();
        var s3 = Substitute.For<IAmazonS3>();
        var service = CreateCoordinator(subscriptions, concerts, s3);

        var subscription = new CalendarSubscriptionDo
        {
            Id = "sub-1",
            IncludeDoorsTime = true,
            IncludeLpStageTime = false,
            LastGeneratedAt = null
        };

        subscriptions.GetByPrimaryKeyAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        concerts.GetLatestChangeAsync(Arg.Any<CancellationToken>()).Returns(DateTimeOffset.UtcNow);
        subscriptions.MarkGeneratedAsync(subscription.Id, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(true);

        await service.GenerateSingleSubscriptionAsync(subscription.Id, CancellationToken.None);

        await s3.Received(1).PutObjectAsync(
            Arg.Is<PutObjectRequest>(request =>
                request.BucketName == "ical-cache-bucket" &&
                request.Key == "sub-1/LinkinPark.ical" &&
                request.ContentType == "text/calendar" &&
                request.Headers.ContentDisposition == "inline; filename=LinkinPark_concerts.ical" &&
                request.Headers.CacheControl == "public, max-age=3600"),
            Arg.Any<CancellationToken>());
        await subscriptions.Received(1).MarkGeneratedAsync(subscription.Id, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateSingleSubscriptionAsync_SkipsGeneration_WhenSubscriptionIsAlreadyUpToDate()
    {
        var subscriptions = Substitute.For<ICalendarSubscriptionRepository>();
        var concerts = Substitute.For<IConcertRepository>();
        var s3 = Substitute.For<IAmazonS3>();
        var service = CreateCoordinator(subscriptions, concerts, s3);

        var now = DateTimeOffset.UtcNow;
        var subscription = new CalendarSubscriptionDo
        {
            Id = "sub-2",
            IncludeDoorsTime = true,
            IncludeLpStageTime = true,
            LastGeneratedAt = now
        };

        subscriptions.GetByPrimaryKeyAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        concerts.GetLatestChangeAsync(Arg.Any<CancellationToken>()).Returns(now.AddMinutes(-1));

        await service.GenerateSingleSubscriptionAsync(subscription.Id, CancellationToken.None);

        await s3.DidNotReceive().PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
        await subscriptions.DidNotReceive()
            .MarkGeneratedAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>());
    }

    private static CalendarSubscriptionGenerationCoordinator CreateCoordinator(
        ICalendarSubscriptionRepository subscriptions,
        IConcertRepository concerts,
        IAmazonS3 s3)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["CalendarSubscriptions:BucketName"] = "ical-cache-bucket",
            ["Quartz:ConnectionString"] = "server=unused;uid=unused;pwd=unused;database=unused"
        }).Build();

        var calendarService = new CalendarGenerationService(concerts);
        return new CalendarSubscriptionGenerationCoordinator(
            subscriptions,
            concerts,
            calendarService,
            s3,
            config,
            NullLogger<CalendarSubscriptionGenerationCoordinator>.Instance);
    }
}

