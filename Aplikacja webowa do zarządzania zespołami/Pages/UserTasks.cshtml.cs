using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserTasksModel : PageModel
    {
        private readonly DatabaseContext _dbContext;

        public UserTasksModel(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            tasksList = new List<Models.Task>();
        }


        public List<Models.Task> tasksList;
        public const string Key = "_userType";
        public const string Key2 = "_userId";
        public const string Key3 = "_groupId";
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty(SupportsGet = true)]
        public int actionTaskId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string feedbackMessage { get; set; }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            tasksList = _dbContext.Tasks.Where(t => t.groups_group_id == groupId && t.users_user_id == userId).ToList();
        }

        public IActionResult OnPostComplete()
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            ////Check if user didn't changed id to an id out of his scope
            ///if there is a task get it and change its status

            ///DODAĆ SPRAWDZANIE DATY UKOŃCZENIA NIE WYŚWIETLAĆ DANYCH STARSZYCH NIZ 7 dni
            if (_dbContext.Tasks.Count(t => t.task_id == actionTaskId && t.groups_group_id == groupId && t.users_user_id == userId && t.status == "nieukończone") > 0)
            {
                var task = _dbContext.Tasks.Where(t => t.task_id == actionTaskId).First();
                task.status = "ukończone";
                task.feedback = feedbackMessage;
                task.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second); ;
                _dbContext.Tasks.Update(task);
                _dbContext.SaveChanges();
            }
            return RedirectToPage("UserTasks");
        }


        //Async methods
        public async Task<Models.Task> GetTaskAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            //Check if user didn't changed id to an id out of his scope
            if ( _dbContext.Tasks.Count(t => t.task_id == id && t.groups_group_id == groupId && t.users_user_id == userId) > 0)
            {
                return await _dbContext.Tasks.FirstAsync(t => t.task_id == id);
            }
            else
            {
                Models.Task emptyTask = new Models.Task();
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
    }
}