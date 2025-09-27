using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

/// <summary>
/// Concert data including bookmarks
/// </summary>
public class ConcertWithBookmarkStatusResponse: Concert
{
    /// <summary>
    /// Status of the bookmarks on the concert
    /// </summary>
    [JsonPropertyName("bookmarkStatus")]
    public required ConcertBookmark BookmarkStatus { get; set; }


    /// <summary>
    /// Creates a new object from the concert and bookmark
    /// </summary>
    /// <param name="concert"></param>
    /// <param name="bookmark"></param>
    /// <returns></returns>
    public static ConcertWithBookmarkStatusResponse FromConcert(Concert concert, ConcertBookmark bookmark)
    {
        return new ConcertWithBookmarkStatusResponse
        {
            BookmarkStatus = bookmark,
            Id = concert.Id,
            ShowType = concert.ShowType,
            CustomTitle = concert.CustomTitle,
            TourName = concert.TourName,
            Status = concert.Status,
            PostedStartTime = concert.PostedStartTime,
            LpuEarlyEntryConfirmed = concert.LpuEarlyEntryConfirmed,
            LpuEarlyEntryTime = concert.LpuEarlyEntryTime,
            DoorsTime = concert.DoorsTime,
            MainStageTime = concert.MainStageTime,
            ExpectedSetDuration = concert.ExpectedSetDuration,
            TimeZoneId = concert.TimeZoneId,
            Country = concert.Country,
            City = concert.City,
            State = concert.State,
            Venue = concert.Venue,
            VenueLatitude = concert.VenueLatitude,
            VenueLongitude = concert.VenueLongitude,
            ScheduleImageFile = concert.ScheduleImageFile
        };
    }
}