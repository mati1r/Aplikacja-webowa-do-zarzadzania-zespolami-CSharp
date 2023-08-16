using Aplikacja_webowa_do_zarządzania_zespołami.Models;
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
            groupList = new List<Models.Groups>();
        }

        public List<Models.Groups> groupList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;

        public void OnGet()
        {
            //Read session data
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);

            //BRAKUJĄCA DANA DO WYŚWIETLENIA TO WŁAŚCICIEL GRUPY !!! NALEŻY W PRZYSZŁOŚCI DODAĆ
            groupList = _dbContext.Groups
                .Where(g => g.Users_Groups.Any(ug => ug.users_user_id == userId && ug.status == "aktywny"))
                .ToList();

        }

        [BindProperty(SupportsGet = true)]
        public int changeGroupId { get; set; }

        public IActionResult OnPostChangeActiveGroup()
        {
            //I need to get values again this time for Post request
            userId = HttpContext.Session.GetInt32(Key2);
            groupList = _dbContext.Groups
                .Where(g => g.Users_Groups.Any(ug => ug.users_user_id == userId && ug.status == "aktywny"))
                .ToList();

            //Check if someone doesn't replaced id with some other id that is not in groupList
            if (groupList.Count(g => g.group_id == changeGroupId) > 0)
            {
                //Check if user is an owner
                if(groupList.Count(g => g.owner_id == userId && g.group_id == changeGroupId) > 0)
                {
                    HttpContext.Session.SetString(Key, "Owner");
                }
                else
                {
                    HttpContext.Session.SetString(Key, "User");
                }
                HttpContext.Session.SetInt32(Key3, changeGroupId);
            }

            return RedirectToPage("Groups");
        }
    }
}
