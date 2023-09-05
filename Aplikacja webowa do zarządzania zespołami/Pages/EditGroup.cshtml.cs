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
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public EditGroupModel(IGroupRepository groupRepository, IUserRepository userRepository)
        {
            _groupRepository = groupRepository;
            _userRepository = userRepository;
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
                group = _groupRepository.GetActiveGroup((int)groupId);
                pendingUsersList = _userRepository.GetPendingUsersList((int)groupId);
                activeUsersList = _userRepository.GetActiveUsersList((int)groupId, (int)userId);
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

            if (_groupRepository.IsGroupNameTaken(groupId, group.name))
            {
                validationErrors.Add("Jest już grupa o tej nazwie");
                return new JsonResult(validationErrors);
            }
            //Check if user didn't deleted session data
            try
            {
                return _groupRepository.EditGroup((int)groupId, group);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostDelete()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Sprawdzić czy osoba usuwająca jest twórcą jezeli jest to usunąć
            if (!_groupRepository.IsUserAnCreator(userId, groupId))
            {
                validationErrors.Add("Użytkownik nie posiada uprawnień do usunięcia grupy");
                return new JsonResult(validationErrors);
            }

            try
            {
                _groupRepository.DeleteGroup((int)groupId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }

            //After removing old group log user to new group if he is part of any
            if (_groupRepository.IsUserAnOwnerOfAnyGroup((int)userId))
            {
                SetSessionData("Owner", _groupRepository.GetOwnerGroupId((int)userId));
            }
            else if (_groupRepository.IsUserActiveMemberOfAnyGroup((int)userId))
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
            if (!_groupRepository.IsUserPendingToJoinGroup(pendingUserId, groupId))
            {
                validationErrors.Add("Wybrany użytkownik nie może być dołączony do grupy");
                return new JsonResult(validationErrors);
            }

            try
            {
                return _groupRepository.AcceptPendingUser((int)groupId, pendingUserId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostRemoveUser()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Check if user didn't changed id
            //Check if user that we are tring to get is: part of a group, have active status, his id is not id of current editing user and he is not an group creator
            if (!_groupRepository.IsActiveUserPartOfGroupExcludeYourselfAndGroupCreator(groupId, userId ,activeUserId))
            {
                validationErrors.Add("Wybrany użytkownik nie może być usuniety z grupy");
                return new JsonResult(validationErrors);
            }

            try
            {
                return _groupRepository.RemoveActiveUserFromGroup((int)groupId, activeUserId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostEditUserRole()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();
            //Check if user didn't changed id
            //Check if user that we are tring to get is: part of a group, have active status, his id is not id of current editing user and he is not an group creator
            if (!_groupRepository.IsActiveUserPartOfGroupExcludeYourselfAndGroupCreator(groupId, userId, activeUserId))
            {
                validationErrors.Add("Wybrany użytkownik nie podlega zmianą");
                return new JsonResult(validationErrors);
            }

            if(activeUserRole != "owner" && activeUserRole != "user")
            {
                validationErrors.Add("Nie istnieje rola którą próbujesz przypisać użytkownikowi");
                return new JsonResult(validationErrors);
            }

            try
            {
                return _groupRepository.EditRoleOActivefUserInGroup((int)groupId, activeUserId, activeUserRole);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        //Partial methods
        public PartialViewResult OnGetEditPartial()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                group = _groupRepository.GetActiveGroup((int)groupId);
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
                pendingUsersList = _userRepository.GetPendingUsersList((int)groupId);
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
                activeUsersList = _userRepository.GetActiveUsersList((int)groupId, (int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialActiveUsers", activeUsersList);
        }

        //Async methods
        public async Task<JsonResult> OnGetActiveUserJsonAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            return new JsonResult(await _groupRepository.GetActiveUserAsync(id, userId, groupId));
        }
    }
}
