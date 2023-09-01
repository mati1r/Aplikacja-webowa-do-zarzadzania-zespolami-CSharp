using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
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
        public string data;
        public int? userId;
        public int? groupId;
        public string? username;

        [BindProperty(SupportsGet = true)]
        public int actionTaskId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string feedbackMessage { get; set; }

        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));
            tasksList = _dbContext.Tasks.Where(t => t.groups_group_id == groupId && t.users_user_id == userId).ToList();
        }

        public IActionResult OnPostComplete()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            ////Check if user didn't changed id to an id out of his scope
            ///if there is a task get it and change its status

            ///DODAĆ SPRAWDZANIE DATY UKOŃCZENIA NIE WYŚWIETLAĆ DANYCH STARSZYCH NIZ 7 dni
            if (_dbContext.Tasks.Any(t => t.task_id == actionTaskId && t.groups_group_id == groupId && t.users_user_id == userId && t.status == "nieukończone"))
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
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            //Check if user didn't changed id to an id out of his scope
            if ( _dbContext.Tasks.Any(t => t.task_id == id && t.groups_group_id == groupId && t.users_user_id == userId))
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