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
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

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

            // Tutaj umieść kod do zapisania nowej grupy

            return new JsonResult("success"); // Sukces
        }
    }
}
