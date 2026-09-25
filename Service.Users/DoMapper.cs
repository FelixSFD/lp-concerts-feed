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
        };
    }
}