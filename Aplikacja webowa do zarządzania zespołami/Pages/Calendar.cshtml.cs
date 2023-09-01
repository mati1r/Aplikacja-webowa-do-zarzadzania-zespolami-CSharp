using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Pages.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class CalendarModel : PageModel
    {

        private readonly DatabaseContext _dbContext;

        public CalendarModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            tasksList = new List<Models.Task>();
        }

        public List<Models.Task> tasksList;
        public string data;
        public int? userId;
        public int? groupId;

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
        }

        //Async methods
        public async Task<JsonResult> OnGetEventsAsync()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            var events = await _dbContext.Tasks
                .Where(t => t.groups_group_id == groupId && t.users_user_id == userId)
                .Select(t => new
                {
                    title = t.task_name,
                    start = t.start_date.ToString("yyyy-MM-dd"),
                    end = t.end_date.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return new JsonResult(events);
        }
    }
}
