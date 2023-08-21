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
            userList = new List<Models.Users>();
        }

        private string error ="";
        private const string Key = "_userType";
        private const string Key2 = "_userId";
        private const string Key3 = "_groupId";
        public List<Models.Tasks> tasksList;
        public List<Models.Users> userList;
        public string data;
        public int? userId;
        public int? groupId;

        [BindProperty(SupportsGet = true)]
        public Tasks createOrEditTask { get; set; }

        [BindProperty(SupportsGet = true)]
        public int deleteTask { get; set; }


        public void OnGet()
        {
            data = HttpContext.Session.GetString(Key);
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            tasksList = _dbContext.Tasks.Where(t => t.groups_group_id == groupId).ToList();

            //Find all users that are active and belong to that group beside owners
            userList = _dbContext.Users
            .Where(g => g.Users_Groups
            .Any(ug => ug.groups_group_id == groupId && ug.role != "owner" && ug.status == "aktywny"))
            .ToList();
        }

        public async Task<Models.Tasks> GetTaskAsync(int id)
        {
            groupId = HttpContext.Session.GetInt32(Key3);

            //Check if requested task is in the same group as owner that is loged in
            if (_dbContext.Tasks.Count(t => t.task_id == id && t.groups_group_id == groupId) > 0)
            {
                return await _dbContext.Tasks.FirstAsync(t => t.task_id == id);
            }
            else
            {
                Tasks emptyTask = new Tasks();
                emptyTask.start_date = DateTime.Now;
                emptyTask.end_date = DateTime.Now;
                emptyTask.task_id = id;
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

        public IActionResult OnPostDelete()
        {
            groupId = HttpContext.Session.GetInt32(Key3);
            List<string> validationErrors = new List<string>();
            //Check if task exists in group
            if (_dbContext.Tasks.Count(t => t.task_id == createOrEditTask.task_id && t.groups_group_id == groupId) == 0)
            {
                validationErrors.Add("Podane zadanie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            Tasks originalTask = _dbContext.Tasks.Where(t => t.task_id == createOrEditTask.task_id).First();
            _dbContext.Remove(originalTask);
            _dbContext.SaveChanges();
            return new JsonResult("success");
        }

        private string IsEndDateHigherThanStartDate(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return "Data początku musi być mniejsza od daty końca";

            return "";
        }

        public IActionResult OnPostAdd()
        {
            //Reset the value of the error and get session values
            error = "";
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            List<string> validationErrors = new List<string>();

            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            error = TaskValidation.IsTaskValid(createOrEditTask.task_name, createOrEditTask.status, createOrEditTask.priority, createOrEditTask.description);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            //Get list of actve users in the group
            userList = _dbContext.Users
                        .Where(g => g.Users_Groups
                        .Any(ug => ug.groups_group_id == groupId && ug.role != "owner" && ug.status == "aktywny"))
                        .ToList();

            //If selected user is on the list of users in the group
            if (userList.Where(ul => ul.user_id == createOrEditTask.users_user_id).Count() == 0)
            {
                //If there is no such a user in the group
                validationErrors.Add("Nie istnieje w grupie użytkownik o podanych danych");
                return new JsonResult(validationErrors);
            }
            //Check if start date is lower than end date
            string isDateCorrect = IsEndDateHigherThanStartDate(createOrEditTask.start_date, createOrEditTask.end_date);
            if (isDateCorrect != "")
            {
                validationErrors.Add(isDateCorrect);
                return new JsonResult(validationErrors);
            }

            //If nothing got returned to this point that means that it validated correctly
            //Check if task is set to complete if so set finish_date
            if (createOrEditTask.status == "ukończone")
            {
                //Setting a date on now and setting seconds to 0
                createOrEditTask.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            }
            createOrEditTask.groups_group_id = (int)groupId;
            _dbContext.Tasks.Add(createOrEditTask);
            _dbContext.SaveChanges();
            return new JsonResult("success");     
        }

        public IActionResult OnPostEdit()
        {
            //Reset the value of the error and get session values
            error = "";
            userId = HttpContext.Session.GetInt32(Key2);
            groupId = HttpContext.Session.GetInt32(Key3);
            List<string> validationErrors = new List<string>();

            //Check if task exists in group
            if (_dbContext.Tasks.Count(t => t.task_id == createOrEditTask.task_id && t.groups_group_id == groupId) == 0)
            {
                validationErrors.Add("Podane zadanie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            //Check if task is completed
            if(_dbContext.Tasks.Count(t=> t.task_id == createOrEditTask.task_id && t.status == "ukończone") > 0)
            {
                validationErrors.Add("Podane zadanie jest już ukończone i nie podlega modyfikacji");
                return new JsonResult(validationErrors);
            }

            //Check if ModelState of send data is valid
            if (!ModelState.IsValid)
            {
                var modelStateValidationErrors = ModelState.ToDictionary(ms => ms.Key,
                   ms => ms.Value.Errors.Select(e => e.ErrorMessage).ToList());

                return new JsonResult(modelStateValidationErrors);
            }

            //Get list of actve users in the group
            userList = _dbContext.Users
                        .Where(g => g.Users_Groups
                        .Any(ug => ug.groups_group_id == groupId && ug.users_user_id != userId && ug.status == "aktywny"))
                        .ToList();

            //If selected user is not on the list of users in the group
            if (userList.Where(ul => ul.user_id == createOrEditTask.users_user_id).Count() == 0)
            {
                validationErrors.Add("Nie istnieje w grupie użytkownik o podanych danych");
                return new JsonResult(validationErrors);
            }

            //If we are here then task exists and is in that group so now we check formating
            error = TaskValidation.IsTaskValid(createOrEditTask.task_name, createOrEditTask.status, createOrEditTask.priority, createOrEditTask.description);
            if (error != "")
            {
                validationErrors.Add(error);
                return new JsonResult(validationErrors);
            }

            //Check if start date is lower than end date
            string isDateCorrect = IsEndDateHigherThanStartDate(createOrEditTask.start_date, createOrEditTask.end_date);
            if (isDateCorrect != "")
            {
                validationErrors.Add(isDateCorrect);
                return new JsonResult(validationErrors);
            }

            //If nothing got returned to this point that means that it validated correctly
            //Now lets get original task and change values aside from task_id, group_id, user_id, and feedback
            //if status changed from nieukończone to ukończone then add finish_date
            Tasks originalTask = _dbContext.Tasks.Where(t => t.task_id == createOrEditTask.task_id).First();
            if (originalTask.status == "nieukończone" && createOrEditTask.status == "ukończone")
            {
                //Setting a date on now and setting seconds to 0
                originalTask.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            }
            originalTask.task_name = createOrEditTask.task_name;
            originalTask.users_user_id = createOrEditTask.users_user_id;
            originalTask.description = createOrEditTask.description;
            originalTask.start_date = createOrEditTask.start_date;
            originalTask.end_date = createOrEditTask.end_date;
            originalTask.status = createOrEditTask.status;
            originalTask.priority = createOrEditTask.priority;
            _dbContext.Update(originalTask);
            _dbContext.SaveChanges();
            return new JsonResult("success");
        }
    }
}
