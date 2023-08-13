using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class AdminTasksModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public AdminTasksModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            tasksList = new List<Models.Tasks>();
        }


        public List<Models.Tasks> tasksList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            Console.WriteLine("ID USER = " + userId);
            Console.WriteLine("ID GRUPY = " + groupId);
            tasksList = _dbContext.Tasks.Where(t => t.groups_group_id == groupId && t.users_user_id == userId).ToList();
        }

        public async Task<Models.Tasks> GetTaskAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            if (_dbContext.Tasks.Count(t => t.task_id == id && t.groups_group_id == groupId && t.users_user_id == userId) > 0)
            {
                return await _dbContext.Tasks.FirstAsync(p => p.task_id == id);
            }
            else
            {
                Tasks emptyTask = new Tasks();
                emptyTask.start_date = DateTime.Now;
                emptyTask.end_date = DateTime.Now;
                emptyTask.task_id = 0;
                emptyTask.task_name = "Błąd";
                emptyTask.description = "Nie znaleziono w bazie określonego zadania";
                emptyTask.status = "Błąd";
                emptyTask.priority = "Błąd";
                return emptyTask;
            }
        }


        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            return new JsonResult(await GetTaskAsync(id));
        }

        [BindProperty(SupportsGet = true)]
        public int actionTaskId { get; set; }

        public IActionResult OnPostSend()
        {
            Console.WriteLine("Id =" + actionTaskId);
            // Wykonaj inne operacje lub przekierowanie
            return RedirectToPage("UserTasks");
        }
    }
}
