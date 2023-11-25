using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplikacja_webowa_do_zarządzania_zespołami.DAO;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class OwnerTasksModel : PageModel
    {
        private readonly ITaskDAO _taskDAO;
        private readonly IUserDAO _userDAO;
        private readonly IGroupDAO _groupDAO;
        public OwnerTasksModel(ITaskDAO taskDAO, IUserDAO userDAO, IGroupDAO groupDAO)
        {
            _taskDAO= taskDAO;
            _userDAO = userDAO;
            _groupDAO = groupDAO;
            tasksList = new List<OwnerTaskPartial>();
            usersList = new List<User>();
        }

        public List<OwnerTaskPartial> tasksList;
        public List<User> usersList;
        private string error ="";
        public string data;
        public int? userId;
        public int? groupId;
        public string? username;
        public string activeGroup;

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

            try
            {
                activeGroup = _groupDAO.GetGroupName((int)groupId);
            }
            catch
            {
                Page();
            }

            tasksList = _taskDAO.GetAllTaskForGroup(groupId);
            //Find all users that are active and belong to that group beside owners
            usersList = _userDAO.GetActiveUsersInGroup(userId, groupId);
        }

        public IActionResult OnPostDelete()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Check if someone (not authorized) doesnt invoke methods in console
            if (!_groupDAO.IsUserAnOwnerOfSelectedGroup(userId, groupId))
            {
                validationErrors.Add("Użytkonik nie posiada uprawnień do operacji");
                return new JsonResult(validationErrors);
            }

            //Check if task exists in group
            if (!_taskDAO.IsExistingTaskInGroup(createOrEditTask, groupId))
            {
                validationErrors.Add("Podane zadanie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            _taskDAO.DeleteTask(createOrEditTask);
            return new JsonResult("success");
        }

        public IActionResult OnPostAdd()
        {
            error = "";
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            //Check if someone (not authorized) doesnt invoke methods in console
            if (!_groupDAO.IsUserAnOwnerOfSelectedGroup(userId, groupId))
            {
                validationErrors.Add("Użytkonik nie posiada uprawnień do operacji");
                return new JsonResult(validationErrors);
            }

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

            usersList = _userDAO.GetActiveUsersInGroup(userId, groupId);

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
                _taskDAO.CreateTask(createOrEditTask, (int)groupId);
                return new JsonResult("success");
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

            //Check if someone (not authorized) doesnt invoke methods in console
            if (!_groupDAO.IsUserAnOwnerOfSelectedGroup(userId, groupId))
            {
                validationErrors.Add("Użytkonik nie posiada uprawnień do operacji");
                return new JsonResult(validationErrors);
            }

            //Check if task exists in group
            if (!_taskDAO.IsExistingTaskInGroup(createOrEditTask,groupId))
            {
                validationErrors.Add("Podane zadanie nie isnieje w tej grupie");
                return new JsonResult(validationErrors);
            }

            //Check if task is completed
            if (_taskDAO.IsTaskCompleted(createOrEditTask))
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
            usersList = _userDAO.GetActiveUsersInGroup(userId, groupId);

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
            _taskDAO.EditTask(createOrEditTask);
            return new JsonResult("success");
        }

        //Partials
        public PartialViewResult OnGetTaskPartial()
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                tasksList = _taskDAO.GetAllTaskForGroup(groupId);
            }   
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialOwnerTasks", tasksList);
        }


        //Async methods
        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));

            if (!_groupDAO.IsUserAnOwnerOfSelectedGroup(userId, groupId))
            {
                Models.Task emptyTask = new Models.Task();
                emptyTask.start_date = DateTime.Now;
                emptyTask.end_date = DateTime.Now;
                emptyTask.task_id = id;
                emptyTask.task_name = "Błąd";
                emptyTask.description = "Nie znaleziono w bazie określonego zadania";
                emptyTask.status = "Błąd";
                emptyTask.priority = "Błąd";

                return new JsonResult(emptyTask);
            }

            return new JsonResult(await _taskDAO.GetTaskAsync(id, groupId));
        }

    }
}
