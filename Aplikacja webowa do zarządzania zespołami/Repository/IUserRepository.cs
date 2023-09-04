using Aplikacja_webowa_do_zarządzania_zespołami.Models;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IUserRepository
    {
        //Messages Page
        List<int> GetIdOfActiveUsersInGroup(int? userId, int? groupId);
        List<User> GetActiveUsersInGroup(int? userId, int? groupId);
    }
}
