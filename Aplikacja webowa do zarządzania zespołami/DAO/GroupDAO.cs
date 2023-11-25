using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.DAO
{
    public class GroupDAO : IGroupDAO
    {
        private readonly DatabaseContext _dbContext;
        public GroupDAO(DatabaseContext dbContext)
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

        public bool IsUserActiveMemberOfSelectedGroup(int? userId, int? selectedGroupId)
        {
            return _dbContext.Users_Groups
                .Where(ugl => ugl.users_user_id == userId && ugl.status == "active")
                .Any(ugl => ugl.groups_group_id == selectedGroupId);
        }

        public bool IsUserAnOwnerOfSelectedGroup(int? userId, int? selectedGroupId)
        {
            return _dbContext.Users_Groups
                .Any(ugl => ugl.users_user_id == userId && ugl.groups_group_id == selectedGroupId && ugl.role == "owner");
        }

        //EditGroup
        public Models.Group GetActiveGroup(int groupId)
        {
            return _dbContext.Groups
                .Where(g => g.group_id == groupId)
                .First();
        }

        public bool IsUserAnCreator(int? userId, int? groupId)
        {
            return _dbContext.Groups.Any(g => g.group_id == groupId && g.owner_id == userId);
        }

        public bool IsGroupNameTakenChange(int? groupId, string name)
        {
            return _dbContext.Groups.Any(g => g.name == name && g.group_id != groupId);
        }

        public void DeleteGroup(int groupId)
        {
            List<Models.Task> groupTasks = _dbContext.Tasks.Where(g => g.groups_group_id == groupId).ToList();
            List<Message> groupMessages = _dbContext.Messages.Where(m => m.groups_group_id == groupId).ToList();
            List<Message_User> groupMessagesUsers = _dbContext.Messages_Users.Where(mu => mu.Messages.groups_group_id == groupId).ToList();
            List<User_Group> groupUsers = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupId).ToList();
            Models.Group group = _dbContext.Groups.Where(g => g.group_id == groupId).First();

            _dbContext.Tasks.RemoveRange(groupTasks);
            _dbContext.Messages_Users.RemoveRange(groupMessagesUsers);
            _dbContext.Messages.RemoveRange(groupMessages);
            _dbContext.Users_Groups.RemoveRange(groupUsers);
            _dbContext.Groups.Remove(group);
            _dbContext.SaveChanges();
        }

        public void EditGroup(int groupId, Models.Group group)
        {
            Models.Group originalGroup = GetActiveGroup(groupId);
            originalGroup.name = group.name;
            originalGroup.description = group.description;
            _dbContext.Update(originalGroup);
            _dbContext.SaveChanges();
        }

        public bool IsUserPendingToJoinGroup(int userId, int? groupId)
        {
            return _dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.users_user_id == userId && ug.status == "pending");
        }

        public void AcceptPendingUser(int groupId, int userId)
        {
            User_Group pendingUser = _dbContext.Users_Groups
                .Where(ug => ug.groups_group_id == groupId && ug.users_user_id == userId)
                .First();

            //We don't need to asigne him a role cause it was asigned during pending request
            pendingUser.status = "active";
            _dbContext.Update(pendingUser);
            _dbContext.SaveChanges();
        }

        public bool IsActiveUserPartOfGroupExcludeYourselfAndGroupCreator(int? groupId, int? userId, int activeUserId)
        {
            return _dbContext.Users
                .Any(u => u.Users_Groups
                .Any(ug => ug.groups_group_id == groupId && ug.status == "active" && ug.Groups.owner_id != activeUserId && userId != activeUserId && ug.users_user_id == activeUserId));
        }

        public void RemoveActiveUserFromGroup(int groupId, int userId)
        {
            User_Group removeUserGroup = _dbContext.Users_Groups
                .Where(ug => ug.groups_group_id == groupId && ug.users_user_id == userId)
                .First();

            _dbContext.Remove(removeUserGroup);
            _dbContext.SaveChanges();
        }

        public void EditRoleOActivefUserInGroup(int groupId, int userId, string newUserRole)
        {
            User_Group editRoleUserGroup = _dbContext.Users_Groups
                .Where(ug => ug.groups_group_id == groupId && ug.users_user_id == userId)
                .First();

            editRoleUserGroup.role = newUserRole;
            _dbContext.Update(editRoleUserGroup);
            _dbContext.SaveChanges();
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

        //Join Group
        public List<GroupJoinPartial> GetGroupsToJoin(int userId)
        {
            return _dbContext.Groups
                .Where(g => !g.Users_Groups.Any(ug => ug.users_user_id == userId))
                .Select(g => new GroupJoinPartial
                {
                    group_id = g.group_id,
                    name = g.name,
                    description = g.description,
                    owner_name = g.Users.username
                })
                .ToList();
        }

        public List<GroupQuitPartial> GetGroupsToQuit(int userId)
        {
            return _dbContext.Groups
                .Where(g => g.owner_id != userId)
                .SelectMany(g => g.Users_Groups.Where(ug => ug.users_user_id == userId && ug.groups_group_id == g.group_id), (g, ug) => new GroupQuitPartial
                {
                    group_id = g.group_id,
                    name = g.name,
                    description = g.description,
                    owner_name = g.Users.username,
                    role = ug.role,
                    status = ug.status
                })
                .ToList();
        }

        public bool WhetherGroupExists(int? groupId)
        {
            return _dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupId);
        }

        public bool IsUserPartOfGroup(int? userId, int? groupId)
        {
            return _dbContext.Users_Groups.Any(ug => ug.users_user_id == userId && ug.groups_group_id == groupId);
        }

        public void AddPendingUserToGroup(int userId, int groupId)
        {
            User_Group userGroup = new User_Group();
            userGroup.groups_group_id = groupId;
            userGroup.users_user_id = userId;
            userGroup.role = "user";
            userGroup.status = "pending";
            _dbContext.Add(userGroup);
            _dbContext.SaveChanges();
        }

        public bool IsGroupNameTaken(string groupName)
        {
            return _dbContext.Groups.Any(g => g.name == groupName);
        }

        public void CreateGroup(Models.Group createGroup, int userId)
        {
            createGroup.owner_id = userId;
            _dbContext.Groups.Add(createGroup);
            _dbContext.SaveChanges();

            User_Group userGroup = new User_Group();

            userGroup.users_user_id = userId;
            userGroup.groups_group_id = createGroup.group_id;
            userGroup.status = "active";
            userGroup.role = "owner";
            _dbContext.Users_Groups.Add(userGroup);
            _dbContext.SaveChanges();
        }

        public async Task<GroupJoinPartial> GetGroupJoinAsync(int id, int? userId)
        {
            bool exists = WhetherGroupExists(id);

            //Check if user didn't changed id to id of a group which he is already a part of (even the one that he is a part on didn' accepted him yet)
            if (_dbContext.Users_Groups.Any(ug => ug.users_user_id != userId && ug.groups_group_id != id) && exists)
            {
                return await _dbContext.Groups
                    .Where(g => g.group_id == id)
                    .Select(g => new GroupJoinPartial
                    {
                        group_id = g.group_id,
                        name = g.name,
                        description = g.description,
                        owner_name = g.Users.username
                    }).FirstAsync();

            }
            GroupJoinPartial emptyGroup = new GroupJoinPartial();
            emptyGroup.group_id = 0;
            emptyGroup.name = "Błąd";
            emptyGroup.owner_name = "Błąd";
            if (!exists)
            {
                emptyGroup.description = "Nie znaleziono grupy";
            }
            else
            {
                emptyGroup.description = "Należysz już do tej grupy";
            }
            return emptyGroup;
        }

        public async Task<GroupQuitPartial> GetGroupQuitAsync(int id, int? userId)
        {
            bool exists = WhetherGroupExists(id);
            bool isCreator = IsUserAnCreator(userId, id);

            //Check if user didn't changed id to id of a group which he isn't part of or he created the group and is current main owner
            if (_dbContext.Users_Groups.Any(ug => ug.users_user_id == userId && ug.groups_group_id == id) && exists && !isCreator)
            {
                return await _dbContext.Groups
                    .Where(g => g.group_id == id)
                    .SelectMany(g => g.Users_Groups.Where(ug => ug.users_user_id == userId && ug.groups_group_id == g.group_id), (g, ug) => new GroupQuitPartial
                    {
                        group_id = g.group_id,
                        name = g.name,
                        description = g.description,
                        owner_name = g.Users.username,
                        role = ug.role,
                        status = ug.status
                    }).FirstAsync();

            }
            GroupQuitPartial emptyGroup = new GroupQuitPartial();
            emptyGroup.group_id = id;
            emptyGroup.name = "Błąd";
            emptyGroup.owner_name = "Błąd";
            emptyGroup.role = "Błąd";
            emptyGroup.status = "Błąd";

            if (!exists)
            {
                emptyGroup.description = "Nie znaleziono grupy";
            }
            if (isCreator)
            {
                emptyGroup.description = "Jesteś twórcą tej grupy";
            }
            return emptyGroup;
        }
    }
}
