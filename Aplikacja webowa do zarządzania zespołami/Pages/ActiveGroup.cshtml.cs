using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class GroupsModel : PageModel
    {
        private readonly DatabaseContext _dbContext;
        public GroupsModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            groupList = new List<Models.Group>();
        }

        public List<Models.Group> groupList;
        public string data;
        public int? userId;
        public int? groupId;
        public string username;
        public string activeGroup;

        [BindProperty(SupportsGet = true)]
        public int changeGroupId { get; set; }

        //OnGet and OnPost methods
        public void OnGet()
        {
            //Read session data
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            try
            {
                groupList = _dbContext.Groups
                    .Where(g => g.Users_Groups.Any(ug => ug.users_user_id == userId && ug.status == "active"))
                    .ToList();
                activeGroup = _dbContext.Groups.Where(g => g.group_id == groupId).Select(g => g.name).First();
            }
            catch
            {
                Page();
            }
        }


        public IActionResult OnPostChangeActiveGroup()
        {
            //I need to get values again this time for Post request
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            //Check if someone doesn't replaced id with some other id that is not in groupList
            if (_dbContext.Users_Groups.Where(ugl => ugl.users_user_id == userId && ugl.status == "active").Any(ugl => ugl.groups_group_id == changeGroupId))
            {
                //Check if user is an owner
                if(_dbContext.Users_Groups.Any(ugl => ugl.users_user_id == userId && ugl.groups_group_id == changeGroupId && ugl.role == "owner"))
                {
                    HttpContext.Session.SetString(ConstVariables.GetKeyValue(1), "Owner");
                }
                else
                {
                    HttpContext.Session.SetString(ConstVariables.GetKeyValue(1), "User");
                }
                HttpContext.Session.SetInt32(ConstVariables.GetKeyValue(3), changeGroupId);
            }

            return RedirectToPage("ActiveGroup");
        }
    }
}
