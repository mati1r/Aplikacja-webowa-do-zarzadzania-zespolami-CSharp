using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class JoinGroupModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public JoinGroupModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            groupList = new List<GroupDTO>();
        }

        public class GroupDTO
        {
            public int group_id { get; set; }
            public string name { get; set; }
            public string description { get; set; }

            public string owner_name { get; set; }
        }

        public List<GroupDTO> groupList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty]
        public int joinGroupId { get; set; }

        [BindProperty]
        public Models.Group createGroup { get; set; }
        public string error;

        private List<GroupDTO> GetGroups(int userId)
        {
            return _dbContext.Groups
                .Where(g => !g.Users_Groups.Any(ug => ug.users_user_id == userId))
                .Select(g => new GroupDTO
                {
                    group_id = g.group_id,
                    name = g.name,
                    description = g.description,
                    owner_name = g.Users.username
                })
                .GroupBy(dto => dto.group_id)
                .Select(group => group.First())
                .ToList();
        }

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);

            try
            {
                groupList = GetGroups((int)userId);
            }
            catch
            {
                Page();
            }
        }

        public async Task<GroupDTO> GetGroupAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);

            Console.WriteLine(userId);

            //Check if group exists
            bool exists = _dbContext.Users_Groups.Any(ug => ug.groups_group_id == id);

            //Check if user didn't changed id to id of a group which he is already a part of (even the one that he is a part on didn' accepted him yet)
            if (_dbContext.Users_Groups.Count(ug => ug.users_user_id != userId && ug.groups_group_id != id) > 0 && exists)
            {
                Console.WriteLine("Jestem debilem3");
                return await _dbContext.Groups
                    .Where(g => g.group_id == id)
                    .Select(g => new GroupDTO
                    {
                        group_id = g.group_id,
                        name = g.name,
                        description = g.description,
                        owner_name = g.Users.username
                    }).FirstAsync();

            }
            GroupDTO emptyGroup = new GroupDTO();
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


        public async Task<JsonResult> OnGetGroupJsonAsync(int id)
        {
            return new JsonResult(await GetGroupAsync(id));
        }

        public PartialViewResult OnGetLoadGroups()
        {
            userId = HttpContext.Session.GetInt32(Key2);

            try
            {
                groupList = GetGroups((int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialJoinGroup", groupList);
        }

        public IActionResult OnPostJoin()
        {
            userId = HttpContext.Session.GetInt32(Key2);
            List<string> validationErrors = new List<string>();

            if(_dbContext.Users_Groups.Any(ug => ug.users_user_id == userId && ug.groups_group_id == joinGroupId))
            {
                validationErrors.Add("Użytkownik znajduje się już w tej grupie");
                return new JsonResult(validationErrors);
            }

            try
            {
                User_Group userGroup = new User_Group();
                userGroup.groups_group_id = joinGroupId;
                userGroup.users_user_id = (int)userId;
                userGroup.role = "user";
                userGroup.status = "nieaktywny";
                _dbContext.Add(userGroup);
                _dbContext.SaveChanges();
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }

            return new JsonResult("success");
        }

        public IActionResult OnPostCreate()
        {
            userId = HttpContext.Session.GetInt32(Key2);
            List<string> validationErrors = new List<string>();

            //Need to clear ModelState to validate only the createGroup model
            ModelState.Clear();
            if (!TryValidateModel(createGroup))
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            if(_dbContext.Groups.Count(g => g.name == createGroup.name) > 0)
            {
                validationErrors.Add("Jest już grupa o tej nazwie");
                return new JsonResult(validationErrors);
            }

            //if there is no such group
            try
            {
                createGroup.owner_id = (int)userId;
                _dbContext.Groups.Add(createGroup);
                _dbContext.SaveChanges();

                User_Group userGroup = new User_Group();

                userGroup.users_user_id = (int)HttpContext.Session.GetInt32(Key2);
                var groupId = _dbContext.Groups.Where(g => g.name == createGroup.name).Select(g => g.group_id).First();
                userGroup.groups_group_id = groupId;
                userGroup.status = "aktywny";
                userGroup.role = "owner";
                _dbContext.Users_Groups.Add(userGroup);
                _dbContext.SaveChanges();
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }

            return new JsonResult("success");
        }
    }
}
