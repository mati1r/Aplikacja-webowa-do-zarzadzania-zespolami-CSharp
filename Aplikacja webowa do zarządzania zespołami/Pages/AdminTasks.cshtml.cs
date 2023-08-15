using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
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

        [BindProperty(SupportsGet = true)]
        public Tasks createOrEditTask { get; set; }


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
            groupId = HttpContext.Session.GetInt32(Key3);
            //Check if requested task is in the same group as owner
            if (_dbContext.Tasks.Count(t => t.task_id == id && t.groups_group_id == groupId) > 0)
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

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.ToDictionary(c => c.Key,
                   c => c.Value.Errors.Select(e => e.ErrorMessage).ToList());

                Console.WriteLine("Nieposzlo");
                return new JsonResult(validationErrors);
            }
            else
            {
                //Someone is trying to send data after failed loaded data so do nothing
                if(createOrEditTask.task_id != 0)
                {
                    //If we are here then task exists and is in that group
                    if(TaskValidation.isPriorityValid(createOrEditTask.priority) && TaskValidation.isStatusValid(createOrEditTask.status)
                        && TaskValidation.IsTaskNameValid(createOrEditTask.task_name) && TaskValidation.IsDescriptionValid(createOrEditTask.description))
                    {
                        //Now lets get original task and change values aside from task_id, group_id, user_id, and feedback
                        //if status changed from nieukończone to ukończone then add finish_date
                        Tasks originalTask = _dbContext.Tasks.Where(t => t.task_id == createOrEditTask.task_id).First();
                        if(originalTask.status == "nieukończone" && createOrEditTask.status == "ukończone")
                        {
                            //Setting a date on now and setting seconds to 0
                            originalTask.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
                        }
                        originalTask.task_name = createOrEditTask.task_name;
                        originalTask.description = createOrEditTask.description;
                        originalTask.start_date = createOrEditTask.start_date;
                        originalTask.end_date = createOrEditTask.end_date;
                        originalTask.status = createOrEditTask.status;
                        originalTask.priority = createOrEditTask.priority;
                        _dbContext.Update(originalTask);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        List<string> validationErrors = new List<string>
                        {
                            "Podane dane nie są w niepoprawnym formacie"
                        };

                        return new JsonResult(validationErrors);
                    }
                }
                else
                {
                    List<string> validationErrors = new List<string>
                        {
                            "Nie można edytować zadania"
                        };

                    return new JsonResult(validationErrors);
                }
                Console.WriteLine("Id =" + createOrEditTask.task_id);
                Console.WriteLine("Poszlo");
                return new JsonResult("success"); // Sukces
            }
        }
    }
}
