using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class JoinGroupModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public JoinGroupModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            groupList = new List<Models.Groups>();
        }

        public List<Models.Groups> groupList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public string data;

        [BindProperty]
        public Groups createGroup { get; set; }
        public string error;

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
        }
        
        public IActionResult OnPostCreate()
        {
            Console.WriteLine("dziala");
            data = HttpContext.Session.GetString(Key);
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new JsonResult(validationErrors);
            }
            else
            {
                //if there is no such group
                if(_dbContext.Groups.Count(g => g.name == createGroup.name) == 0)
                {
                    createGroup.owner_id = (int)HttpContext.Session.GetInt32(Key2);

                    _dbContext.Groups.Add(createGroup);
                    _dbContext.SaveChanges();

                    Users_Groups userGroup = new Users_Groups();

                    userGroup.users_user_id = (int)HttpContext.Session.GetInt32(Key2);
                    var groupId = _dbContext.Groups.Where(g => g.name == createGroup.name).Select(g => g.group_id).First();
                    userGroup.groups_group_id = groupId;
                    userGroup.status = "aktywny";
                    _dbContext.Users_Groups.Add(userGroup);
                    _dbContext.SaveChanges();
                }
                else
                {
                    //send error
                    List<string> validationErrors = new List<string>();
                    validationErrors.Add("Jest już grupa o tej nazwie");

                    return new JsonResult(validationErrors);
                }
            }

            // Tutaj umieść kod do zapisania nowej grupy

            return new JsonResult("success"); // Sukces
        }
    }
}
