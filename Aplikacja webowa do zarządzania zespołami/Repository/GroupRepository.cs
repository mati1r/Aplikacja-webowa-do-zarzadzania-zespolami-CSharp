using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DatabaseContext _dbContext;
        public GroupRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        //Login and EditGroup
        public bool IsUserAnOwnerOfAnyGroup(int userId)
        {
            return _dbContext.Users_Groups
                .Where(ugl => ugl.role == "owner").Any(ugl => ugl.users_user_id == userId);
        }

        public int GetOwnerGroupId(int userId)
        {
            return _dbContext.Users_Groups
                .Where(ugl => ugl.users_user_id == userId && ugl.role == "owner")
                .Select(ugl => ugl.groups_group_id).First();
        }

        public bool IsUserActiveMemberOfAnyGroup(int userId)
        {
            return _dbContext.Users_Groups
                .Any(ugl => ugl.users_user_id == userId && ugl.status == "active");
        }

        public int GetUserGroupId(int userId)
        {
            return _dbContext.Users_Groups
                .Where(ugl => ugl.users_user_id == userId && ugl.status == "active" && ugl.role == "user")
                .Select(ug => ug.groups_group_id)
                .First();
        }

        //ActiveGroup
        public List<ActiveGroupDTO> GetGroupsForActiveUser(int userId)
        {
            return _dbContext.Groups
                .Where(g => g.Users_Groups.Any(ug => ug.users_user_id == userId && ug.status == "active"))
                .Select(g => new ActiveGroupDTO
                {
                    group_id = g.group_id,
                    name = g.name,
                    description = g.description,
                    owner_name = g.Users.username
                })
                .ToList();
        }

        public string GetGroupName(int groupId)
        {
            return _dbContext.Groups
                .Where(g => g.group_id == groupId)
                .Select(g => g.name)
                .First();
        }

        public bool IsUserActiveMemberOfSelectedGroup(int? userId, int selectedGroupId)
        {
            return _dbContext.Users_Groups
                .Where(ugl => ugl.users_user_id == userId && ugl.status == "active")
                .Any(ugl => ugl.groups_group_id == selectedGroupId);
        }

        public bool IsUserAnOwnerOfSelectedGroup(int? userId, int selectedGroupId)
        {
            return _dbContext.Users_Groups
                .Any(ugl => ugl.users_user_id == userId && ugl.groups_group_id == selectedGroupId && ugl.role == "owner");
        }

        //EditGroup
        public Group GetActiveGroup(int groupId)
        {
            return _dbContext.Groups
                .Where(g => g.group_id == groupId)
                .First();
        }

        public bool IsUserAnCreator(int? userId, int? groupId)
        {
            return _dbContext.Groups.Any(g => g.group_id == groupId && g.owner_id == userId);
        }

        public bool IsGroupNameTaken(int? groupId, string name)
        {
            return _dbContext.Groups.Any(g => g.name == name && g.group_id != groupId);
        }

        public void DeleteGroup(int groupId)
        {
            List<Models.Task> groupTasks = _dbContext.Tasks.Where(g => g.groups_group_id == groupId).ToList();
            List<Message> groupMessages = _dbContext.Messages.Where(m => m.groups_group_id == groupId).ToList();
            List<Message_User> groupMessagesUsers = _dbContext.Messages_Users.Where(mu => mu.Messages.groups_group_id == groupId).ToList();
            List<User_Group> groupUsers = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupId).ToList();
            Group group = _dbContext.Groups.Where(g => g.group_id == groupId).First();

            _dbContext.Tasks.RemoveRange(groupTasks);
            _dbContext.Messages_Users.RemoveRange(groupMessagesUsers);
            _dbContext.Messages.RemoveRange(groupMessages);
            _dbContext.Users_Groups.RemoveRange(groupUsers);
            _dbContext.Groups.Remove(group);
            _dbContext.SaveChanges();
        }

        public JsonResult EditGroup(int groupId, Group group)
        {
            Group originalGroup = GetActiveGroup(groupId);
            originalGroup.name = group.name;
            originalGroup.description = group.description;
            _dbContext.Update(originalGroup);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public bool IsUserPendingToJoinGroup(int userId, int? groupId)
        {
            return _dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.users_user_id == userId && ug.status == "pending");
        }

        public JsonResult AcceptPendingUser(int groupId, int userId)
        {
            User_Group pendingUser = _dbContext.Users_Groups
                .Where(ug => ug.groups_group_id == groupId && ug.users_user_id == userId)
                .First();

            //We don't need to asigne him a role cause it was asigned during pending request
            pendingUser.status = "active";
            _dbContext.Update(pendingUser);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public bool IsActiveUserPartOfGroupExcludeYourselfAndGroupCreator(int? groupId, int? userId, int activeUserId)
        {
            return _dbContext.Users
                .Any(u => u.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.status == "active" && ug.Groups.owner_id != activeUserId && userId != activeUserId && ug.users_user_id == activeUserId));
        }

        public JsonResult RemoveActiveUserFromGroup(int groupId, int userId)
        {
            User_Group removeUserGroup = _dbContext.Users_Groups
                .Where(ug => ug.groups_group_id == groupId && ug.users_user_id == userId)
                .First();

            _dbContext.Remove(removeUserGroup);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public JsonResult EditRoleOActivefUserInGroup(int groupId, int userId, string newUserRole)
        {
            User_Group editRoleUserGroup = _dbContext.Users_Groups
                .Where(ug => ug.groups_group_id == groupId && ug.users_user_id == userId)
                .First();

            editRoleUserGroup.role = newUserRole;
            _dbContext.Update(editRoleUserGroup);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public async Task<ActiveUserDTO> GetActiveUserAsync(int id, int? userId, int? groupId)
        {
            if (IsActiveUserPartOfGroupExcludeYourselfAndGroupCreator(groupId, userId, id))
            {
                return await _dbContext.Users.Where(u => u.user_id == id)
                    .SelectMany(u => u.Users_Groups.Where(ug => ug.groups_group_id == groupId && ug.users_user_id == id), (u, ug) => new ActiveUserDTO
                    {
                        user_id = u.user_id,
                        username = u.username,
                        e_mail = u.e_mail,
                        role = ug.role
                    }).FirstAsync();
            }
            else
            {
                ActiveUserDTO emptyUser = new ActiveUserDTO();
                emptyUser.user_id = 0;
                emptyUser.username = "Błąd";
                emptyUser.e_mail = "Błąd";
                emptyUser.role = "user";

                return emptyUser;
            }
        }
    }
}
