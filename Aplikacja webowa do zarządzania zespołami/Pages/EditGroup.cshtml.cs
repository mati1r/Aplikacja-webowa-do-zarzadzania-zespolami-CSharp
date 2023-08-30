using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Aplikacja_webowa_do_zarządzania_zespołami.Pages.JoinGroupModel;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class EditGroupModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public EditGroupModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            Group group = new Group();
            List<User> pendingUsersList = new List<User>();
            List<User> activeUsersList = new List<User>();
        }

        public List<User> pendingUsersList;
        public List<User> activeUsersList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty]
        public Group group { get; set; }

        [BindProperty]
        public int pendingUserId { get; set; }

        [BindProperty]
        public int activeUserId { get; set; }

        private Group GetActiveGroup(int groupId)
        {
            return _dbContext.Groups.Where(g => g.group_id == groupId).First();
        }

        private List<User> GetPendingUsersList(int groupId)
        {
            return _dbContext.Users.Where(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "nieaktywny")).ToList();
        }

        private List<User> GetActiveUsersList(int groupId, int userId)
        {
            return _dbContext.Users.Where(u => u.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.status == "aktywny" 
                                          && ug.users_user_id != ug.Groups.owner_id && ug.users_user_id != userId)).ToList();
        }

        //On get and post methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

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
            groupId = HttpContext.Session.GetInt32(Key3);
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

        public IActionResult OnPostAcceptPendingUser()
        {
            groupId = HttpContext.Session.GetInt32(Key3);
            List<string> validationErrors = new List<string>();
            //Check if user is in that group and his status is pending
            if (!_dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupId && ug.users_user_id == pendingUserId && ug.status == "nieaktywny"))
            {
                validationErrors.Add("Wybrany użytkownik nie może być dołączony do grupy");
                return new JsonResult(validationErrors);
            }

            try
            {
                User_Group pendingUser = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupId && ug.users_user_id == pendingUserId).First();

                //We don't need to asigne him a role cause it was asigned during pending request
                pendingUser.status = "aktywny";
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

        //Partial methods
        public PartialViewResult OnGetEditPartial()
        {
            groupId = HttpContext.Session.GetInt32(Key3);

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
            groupId = HttpContext.Session.GetInt32(Key3);

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
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

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
    }
}
