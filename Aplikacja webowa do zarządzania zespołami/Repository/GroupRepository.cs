using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DatabaseContext _dbContext;
        public GroupRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        //Login
        public bool IsUserAnOwner(LoginUserDTO userData)
        {
            return _dbContext.Users_Groups.Where(ugl => ugl.role == "owner").Any(ugl => ugl.users_user_id == userData.userId);
        }

        public int GetOwnerGroupId(LoginUserDTO userData)
        {
            return _dbContext.Users_Groups
                    .Where(ugl => ugl.users_user_id == userData.userId && ugl.role == "owner")
                    .Select(ugl => ugl.users_user_id).First();
        }

        public bool IsUserActiveMemberOfGroup(LoginUserDTO userData)
        {
            return _dbContext.Users_Groups.Any(ugl => ugl.users_user_id == userData.userId && ugl.status == "active");
        }

        public int GetUserGroupId(LoginUserDTO userData)
        {
            return _dbContext.Users_Groups
                    .Where(ugl => ugl.users_user_id == userData.userId && ugl.status == "active" && ugl.role == "user")
                    .Select(ug => ug.groups_group_id)
                    .First();
        }
    }
}
