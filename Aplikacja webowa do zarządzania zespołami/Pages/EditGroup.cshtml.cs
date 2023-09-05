using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class EditGroupModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        private readonly IGroupRepository _groupRepository;

        public EditGroupModel(DatabaseContext dbContext, IGroupRepository groupRepository)
        {
            _dbContext = dbContext;
            _groupRepository = groupRepository;
            group = new Group();
            pendingUsersList = new List<User>();
            activeUsersList = new List<User>();
        }

        public List<User> pendingUsersList;
        public List<User> activeUsersList;
        public string data;
        public int? userId;
        public int? groupId;
        public string username;

        [BindProperty]
        public Group group { get; set; }

        [BindProperty]
        public int pendingUserId { get; set; }

        [BindProperty]
        public int activeUserId { get; set; }

        [BindProperty]
        public string activeUserRole { get; set; }

        private Group GetActiveGroup(int groupId)
        {
            return _dbContext.Groups.Where(g => g.group_id == groupId).First();
        }

        private List<User> GetPendingUsersList(int groupId)
        {
            return _dbContext.Users.Where(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "pending")).ToList();
        }

        private List<User> GetActiveUsersList(int groupId, int userId)
        {
            return _dbContext.Users.Where(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "active" 
                                          && ug.users_user_id != ug.Groups.owner_id && ug.users_user_id != userId)).ToList();
        }

        //Private methods
        private void SetSessionData(string userType, int groupId)
        {
            HttpContext.Session.SetString(ConstVariables.GetKeyValue(1), userType);
            HttpContext.Session.SetInt32(ConstVariables.GetKeyValue(3), groupId);
        }

        //On get and post methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                group = GetActiveGroup((int)groupId);
                pendingUsersList = GetPendingUsersList((int)groupId);
                activeUsersList = GetActiveUsersList((int)groupId, (int)userId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostEdit()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Need to clear ModelState to validate only the userPersonalData model
            ModelState.Clear();
            if (!TryValidateModel(group))
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            if (_dbContext.Groups.Any(g => g.name == group.name && g.group_id != groupId))
            {
                validationErrors.Add("Jest już grupa o tej nazwie");
                return new JsonResult(validationErrors);
            }

            try
            {
                Group originalGroup = GetActiveGroup((int)groupId);
                originalGroup.name = group.name;
                originalGroup.description = group.description;
                _dbContext.Update(originalGroup);
                _dbContext.SaveChanges();
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);

            }
            return new JsonResult("success");
        }

        private bool IsUserAnCreator(int? userId, int? groupId)
        {
            return _dbContext.Groups.Any(g => g.group_id == groupId && g.owner_id == userId);
        }

        public IActionResult OnPostDelete()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Sprawdzić czy osoba usuwająca jest twórcą jezeli jest to usunąć
            if (!IsUserAnCreator(userId, groupId))
            {
                validationErrors.Add("Użytkownik nie posiada uprawnień do usunięcia grupy");
                return new JsonResult(validationErrors);
            }

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

            //After removing old group log user to new group
            if (_groupRepository.IsUserAnOwner((int)userId))
            {
                SetSessionData("Owner", _groupRepository.GetOwnerGroupId((int)userId));
            }
            else if (_groupRepository.IsUserActiveMemberOfGroup((int)userId))
            {
                SetSessionData("User", _groupRepository.GetUserGroupId((int)userId));
            }
            else
            {
                SetSessionData("User", 0);
            }

            return new JsonResult("success");
        }

        public IActionResult OnPostAcceptPendingUser()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Check if user is in that group and his status is pending
            if (!_dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.users_user_id == pendingUserId && ug.status == "pending"))
            {
                validationErrors.Add("Wybrany użytkownik nie może być dołączony do grupy");
                return new JsonResult(validationErrors);
            }

            try
            {
                User_Group pendingUser = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupId && ug.users_user_id == pendingUserId).First();

                //We don't need to asigne him a role cause it was asigned during pending request
                pendingUser.status = "active";
                _dbContext.Update(pendingUser);
                _dbContext.SaveChanges();
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }

            return new JsonResult("success");
        }

        public IActionResult OnPostRemoveUser()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Check if user didn't changed id
            //Check if user that we are tring to get is: part of a group, have active status, his id is not id of current editing user and he is not an group creator
            if (!_dbContext.Users.Any(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "active"
                                          && ug.Groups.owner_id != activeUserId && userId != activeUserId && ug.users_user_id == activeUserId)))
            {
                validationErrors.Add("Wybrany użytkownik nie może być usuniety z grupy");
                return new JsonResult(validationErrors);
            }


            User_Group removeUserGroup = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupId && ug.users_user_id == activeUserId).First();
            _dbContext.Remove(removeUserGroup);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        public IActionResult OnPostEditUserRole()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Check if user didn't changed id
            //Check if user that we are tring to get is: part of a group, have active status, his id is not id of current editing user and he is not an group creator
            if (!_dbContext.Users.Any(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "active"
                                          && ug.Groups.owner_id != activeUserId && userId != activeUserId && ug.users_user_id == activeUserId)))
            {
                validationErrors.Add("Wybrany użytkownik nie podlega zmianą");
                return new JsonResult(validationErrors);
            }

            if(activeUserRole != "owner" && activeUserRole != "user")
            {
                validationErrors.Add("Nie istnieje rola którą próbujesz przypisać użytkownikowi");
                return new JsonResult(validationErrors);
            }

            Console.WriteLine("ROLA = " + activeUserRole);

            User_Group editRoleUserGroup = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupId && ug.users_user_id == activeUserId).First();
            editRoleUserGroup.role = activeUserRole;
            _dbContext.Update(editRoleUserGroup);
            _dbContext.SaveChanges();

            return new JsonResult("success");
        }

        //Partial methods
        public PartialViewResult OnGetEditPartial()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                group = GetActiveGroup((int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialEditGroup", group);
        }

        public PartialViewResult OnGetPendingUsersPartial()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                pendingUsersList = GetPendingUsersList((int)groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialPendingUsers", pendingUsersList);
        }

        public PartialViewResult OnGetActiveUsersPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                activeUsersList = GetActiveUsersList((int)groupId, (int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialActiveUsers", activeUsersList);
        }

        //Async methods
        public async Task<ActiveUserDTO> GetActiveUserAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            //Check if user didn't changed id
            //Check if user that we are tring to get is: part of a group, have active status, his id is not id of current editing user and he is not an group creator
            if (_dbContext.Users.Any(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "active"
                                          && ug.Groups.owner_id != id && userId != id && ug.users_user_id == id)))
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

        public async Task<JsonResult> OnGetActiveUserJsonAsync(int id)
        {
            return new JsonResult(await GetActiveUserAsync(id));
        }
    }
}
