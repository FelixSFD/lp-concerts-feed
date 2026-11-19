using Database.Users.DataObjects;
using Service.Users.DataStructure;

namespace Service.Users;

public static class DoMapper
{
    /// <summary>
    /// Convert a data object to its business object.
    /// </summary>
    /// <param name="dataObject"></param>
    /// <returns></returns>
    public static UserBo ToBo(this UserDo dataObject)
    {
        return new UserBo
        {
            Id = dataObject.Id,
            Username = dataObject.Username,
            OriginCountryCode = dataObject.OriginCountryCode,
            CreatedAt = dataObject.CreatedAt,
            UpdatedAt = dataObject.UpdatedAt,
        };
    }

    /// <summary>Maps a calendar subscription data object to its business representation.</summary>
    /// <param name="dataObject">The subscription to map.</param>
    /// <returns>The mapped subscription.</returns>
    public static CalendarSubscriptionBo ToBo(this CalendarSubscriptionDo dataObject)
    {
        return new CalendarSubscriptionBo(
            dataObject.Id,
            dataObject.UserId,
            dataObject.IncludeDoorsTime,
            dataObject.IncludeLpStageTime,
            dataObject.LastGeneratedAt,
            dataObject.CreatedAt,
            dataObject.UpdatedAt);
    }
}