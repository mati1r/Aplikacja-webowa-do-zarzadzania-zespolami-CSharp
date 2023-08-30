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
        }

        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty]
        public Group group { get; set; }

        private Group GetActiveGroup(int groupId)
        {
            return _dbContext.Groups.Where(g => g.group_id == groupId).First();
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

        //Partial methods
        public PartialViewResult OnGetLoadEditPartial()
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
    }
}
