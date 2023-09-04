using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _dbContext;
        public UserRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<User> GetActiveUsersInGroup(int? userId, int? groupId)
        {
            return _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "active"))
                .ToList();
        }

        public List<int> GetIdOfActiveUsersInGroup(int? userId, int? groupId)
        {
            return _dbContext.Users
                .Where(g => g.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "active"))
                .Select(g => g.user_id).ToList();
        }
    }
}
