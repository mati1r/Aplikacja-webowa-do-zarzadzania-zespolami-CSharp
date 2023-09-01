using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
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
            groupJoinList = new List<GroupJoinPartial>();
            groupQuitList = new List<GroupQuitPartial>();
        }

        public List<GroupJoinPartial> groupJoinList;
        public List<GroupQuitPartial> groupQuitList;
        public string data;
        public int? userId;
        public string username;

        [BindProperty]
        public int groupJoinId { get; set; }

        [BindProperty]
        public int groupQuitId { get; set; }

        [BindProperty]
        public Models.Group createGroup { get; set; }
        public string error;


        //private return Lists methods
        private List<GroupJoinPartial> GetGroupsToJoin(int userId)
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
                .GroupBy(gb => gb.group_id)
                .Select(group => group.First())
                .ToList();
        }

        private List<GroupQuitPartial> GetGroupsToQuit(int userId)
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
                .GroupBy(gb => gb.group_id)
                .Select(group => group.First())
                .ToList();
        }

        //On get and post methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                groupJoinList = GetGroupsToJoin((int)userId);
                groupQuitList = GetGroupsToQuit((int)userId);
            }
            catch
            {
                Page();
            }
        }

        public IActionResult OnPostJoin()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            List<string> validationErrors = new List<string>();

            //Check if group exists
            if(!_dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupJoinId))
            {
                validationErrors.Add("Podana grupa nie istnieje");
                return new JsonResult(validationErrors);
            }

            if(_dbContext.Users_Groups.Any(ug => ug.users_user_id == userId && ug.groups_group_id == groupJoinId))
            {
                validationErrors.Add("Użytkownik znajduje się już w tej grupie");
                return new JsonResult(validationErrors);
            }

            try
            {
                User_Group userGroup = new User_Group();
                userGroup.groups_group_id = groupJoinId;
                userGroup.users_user_id = (int)userId;
                userGroup.role = "user";
                userGroup.status = "pending";
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

        public IActionResult OnPostQuit()
        {
            //Sprawdzić czy istnieje grupa z której użytkownik chce wyjść oraz czy jest on jej członkiem
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            List<string> validationErrors = new List<string>();

            //Check if user is part of a group
            if( !_dbContext.Users_Groups.Any(ug => ug.groups_group_id == groupQuitId && ug.users_user_id == userId))
            {
                validationErrors.Add("Użytkownik nie znajduje sie w tej grupie");
                return new JsonResult(validationErrors);
            }

            //Check if user is not a creator
            if(_dbContext.Groups.Any(g => g.group_id == groupQuitId && g.owner_id  == userId))
            {
                validationErrors.Add("Twórca grupy nie może z niej wyjść");
                return new JsonResult(validationErrors);
            }

            User_Group userGroup = _dbContext.Users_Groups.Where(ug => ug.groups_group_id == groupQuitId && ug.users_user_id == userId).First();
            _dbContext.Remove(userGroup);
            _dbContext.SaveChanges();       

            return new JsonResult("success");
        }

        public IActionResult OnPostCreate()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            if(_dbContext.Groups.Any(g => g.name == createGroup.name))
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

                userGroup.users_user_id = (int)HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
                userGroup.groups_group_id = _dbContext.Groups.Where(g => g.name == createGroup.name).Select(g => g.group_id).First();
                userGroup.status = "active";
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

        //Partial methods
        public PartialViewResult OnGetGroupsJoinPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            try
            {
                groupJoinList = GetGroupsToJoin((int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialGroupsJoin", groupJoinList);
        }

        public PartialViewResult OnGetGroupsQuitPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            try
            {
                groupQuitList = GetGroupsToQuit((int)userId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialGroupsQuit", groupQuitList);
        }

        //Async Methods

        //Join group method
        public async Task<JsonResult> OnGetGroupJoinJsonAsync(int id)
        {
            return new JsonResult(await GetGroupJoinAsync(id));
        }

        private async Task<GroupJoinPartial> GetGroupJoinAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            //Check if group exists
            bool exists = _dbContext.Users_Groups.Any(ug => ug.groups_group_id == id);

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

        //Quit group method
        public async Task<JsonResult> OnGetGroupQuitJsonAsync(int id)
        {
            return new JsonResult(await GetGroupQuitAsync(id));
        }

        private async Task<GroupQuitPartial> GetGroupQuitAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            //Check if group exists
            bool exists = _dbContext.Users_Groups.Any(ug => ug.groups_group_id == id);
            bool isOwner = _dbContext.Groups.Any(g => g.owner_id == userId && g.group_id == id);

            //Check if user didn't changed id to id of a group which he isn't part of or he created the group and is current main owner
            if (_dbContext.Users_Groups.Any(ug => ug.users_user_id == userId && ug.groups_group_id == id) && exists && !isOwner)
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
            if (isOwner)
            {
                emptyGroup.description = "Jesteś twórcą tej grupy";
            }
            return emptyGroup;
        }
    }
}
