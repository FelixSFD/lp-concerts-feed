using Common.Utils.Exceptions;

namespace Service.Users.Exceptions;

public class UserNotFoundException(string userId) : NotFoundExceptionBase("User", userId)
{
}