using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class OwnerTasksModel : PageModel
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        public OwnerTasksModel(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            tasksList = new List<Models.Task>();
            usersList = new List<User>();
        }

        public List<Models.Task> tasksList;
        public List<User> usersList;
        private string error ="";
        public string data;
        public int? userId;
        public int? groupId;
        public string? username;

        [BindProperty(SupportsGet = true)]
        public Models.Task createOrEditTask { get; set; }

        [BindProperty(SupportsGet = true)]
        public int deleteTask { get; set; }

        //Private methods
        private bool IsSelectedUserIsOnTheList(List<User> usersList)
        {
            return usersList.Any(ul => ul.user_id == createOrEditTask.users_user_id);
        }
        private string IsEndDateHigherThanStartDate(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return "Data początku musi być mniejsza od daty końca";

            return "";
        }
        //OnGet and OnPost methods
        public void OnGet()
        {
            data = HttpContext.Session.GetString(ConstVariables.GetKeyValue(1));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            username = HttpContext.Session.GetString(ConstVariables.GetKeyValue(4));

            tasksList = _taskRepository.GetAllTaskForGroup(groupId);

            //Find all users that are active and belong to that group beside owners
            usersList = _userRepository.GetActiveUsersInGroup(userId, groupId);
        }

        public IActionResult OnPostDelete()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Check if task exists in group
            if (!_taskRepository.IsExistingTaskInGroup(createOrEditTask, groupId))
            {
                validationErrors.Add("Podane zadanie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            return _taskRepository.DeleteTask(createOrEditTask);
        }

        public IActionResult OnPostAdd()
        {
            error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
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

            usersList = _userRepository.GetActiveUsersInGroup(userId, groupId);

            if (!IsSelectedUserIsOnTheList(usersList))
            {
                validationErrors.Add("Nie istnieje w grupie użytkownik o podanych danych");
                return new JsonResult(validationErrors);
            }
            string isDateCorrect = IsEndDateHigherThanStartDate(createOrEditTask.start_date, createOrEditTask.end_date);
            if (isDateCorrect != "")
            {
                validationErrors.Add(isDateCorrect);
                return new JsonResult(validationErrors);
            }

            //Check if user didn't deleted session data
            try
            {
                return _taskRepository.CreateTask(createOrEditTask, (int)groupId);
            }
            catch
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
        }

        public IActionResult OnPostEdit()
        {
            //Reset the value of the error and get session values
            error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Check if task exists in group
            if (!_taskRepository.IsExistingTaskInGroup(createOrEditTask,groupId))
            {
                validationErrors.Add("Podane zadanie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            //Check if task is completed
            if (_taskRepository.IsTaskCompleted(createOrEditTask))
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
            usersList = _userRepository.GetActiveUsersInGroup(userId, groupId);

            //If selected user is not on the list of users in the group
            if (!IsSelectedUserIsOnTheList(usersList))
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
            return _taskRepository.EditTask(createOrEditTask);
        }


        //Async methods
        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            return new JsonResult(await _taskRepository.GetTaskAsync(id, groupId));
        }

    }
}
