using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages.TestPages
{
    public class TestModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public TestModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            tasksList = new List<Models.Tasks>();
            task = new Tasks();
        }

        public List<Models.Tasks> tasksList;
        public Tasks task;
        private const string Key = "_userType";
        public string data;
        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
        }

        public PartialViewResult OnGetPartialTest(string id)
        {
            int intId = int.Parse(id);
            task = _dbContext.Tasks.Where(t => t.users_user_id == intId).First();
            return Partial("Partials/_PartialTest", task);
        }
    }
}
