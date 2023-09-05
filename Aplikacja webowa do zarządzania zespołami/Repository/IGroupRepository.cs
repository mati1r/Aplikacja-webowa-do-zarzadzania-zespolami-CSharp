using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface IGroupRepository
    {
        //Login Page
        bool IsUserAnOwnerOfAnyGroup(int userId);
        int GetOwnerGroupId(int userId);
        bool IsUserActiveMemberOfAnyGroup(int userId);
        int GetUserGroupId(int userId);

        //Active Group Page
        List<ActiveGroupDTO> GetGroupsForActiveUser(int userId);
        string GetGroupName(int groupId);
        bool IsUserActiveMemberOfSelectedGroup(int? userId, int selectedGroupId);
        bool IsUserAnOwnerOfSelectedGroup(int? userId, int selectedGroupId);

        //Edit Group Page
        Group GetActiveGroup(int groupId);
        bool IsUserAnCreator(int? userId, int? groupId);
        bool IsGroupNameTakenChange(int? groupId, string name);
        void DeleteGroup(int groupId);
        JsonResult EditGroup(int groupId, Group group);
        bool IsUserPendingToJoinGroup(int userId, int? groupId);
        JsonResult AcceptPendingUser(int groupId, int userId);
        bool IsActiveUserPartOfGroupExcludeYourselfAndGroupCreator(int? groupId, int? userId, int activeUserId);
        JsonResult RemoveActiveUserFromGroup(int groupId, int userId);
        JsonResult EditRoleOActivefUserInGroup(int groupId, int userId, string newUserRole);
        Task<ActiveUserDTO> GetActiveUserAsync(int id, int? userId, int? groupId);

        //Join Group Page
        List<GroupJoinPartial> GetGroupsToJoin(int userId);
        List<GroupQuitPartial> GetGroupsToQuit(int userId);
        bool WhetherGroupExists(int? groupId);
        bool IsUserPartOfGroup(int? userId, int? groupId);
        JsonResult AddPendingUserToGroup(int userId, int groupId);
        bool IsGroupNameTaken(string groupName);
        JsonResult CreateGroup(Models.Group createGroup, int userId);
        Task<GroupJoinPartial> GetGroupJoinAsync(int id, int? userId);
        Task<GroupQuitPartial> GetGroupQuitAsync(int id, int? userId);
    }
}
