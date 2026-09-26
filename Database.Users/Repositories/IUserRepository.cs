using Common.Database.Repositories;
using Database.Users.DataObjects;

namespace Database.Users.Repositories;

public interface IUserRepository : ISingleKeyRepositoryBase<UserDo, string>, IRepositoryBase<UserDo>
{
}