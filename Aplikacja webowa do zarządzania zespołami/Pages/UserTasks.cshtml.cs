using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aplikacja_webowa_do_zarządzania_zespołami.Repository;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Pages
{
    public class UserTasksModel : PageModel
    {
        private readonly ITaskRepository _taskRepository;

        public UserTasksModel(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
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

            tasksList = _taskRepository.GetTasksForUser(userId, groupId);
        }

        public IActionResult OnPostComplete()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            ////Check if user didn't changed id to an id out of his scope
            ///if there is a task get it and change its status        
            if (!_taskRepository.IsTaskForUserNotComplete(actionTaskId, userId, groupId))
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
            _taskRepository.CompleteTask(actionTaskId, feedbackMessage);
            return new JsonResult("success");
        }

        public IActionResult OnPostCurrent()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            ////Check if user didn't changed id to an id out of his scope
            ///if there is a task get it and change its status        
            if (!_taskRepository.IsTaskForUserNotComplete(actionTaskId, userId, groupId))
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
            _taskRepository.CurrentTask(actionTaskId, feedbackMessage);
            return new JsonResult("success");
        }

        public IActionResult OnPostNotComplete()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            List<string> validationErrors = new List<string>();

            ////Check if user didn't changed id to an id out of his scope
            ///if there is a task get it and change its status        
            if (!_taskRepository.IsTaskForUserNotComplete(actionTaskId, userId, groupId))
            {
                validationErrors.Add("Wystąpił błąd");
                return new JsonResult(validationErrors);
            }
            _taskRepository.NotCompleteTask(actionTaskId, feedbackMessage);
            return new JsonResult("success");
        }

        //Partials
        public PartialViewResult OnGetTaskPartial()
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));

            try
            {
                tasksList = _taskRepository.GetTasksForUser(userId, groupId);
            }
            catch
            {
                Page();
            }

            return Partial("Partials/_PartialUserTasks", tasksList);
        }


        //Async methods

        public async Task<JsonResult> OnGetTaskJsonAsync(int id)
        {
            userId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(2));
            groupId = HttpContext.Session.GetInt32(ConstVariables.GetKeyValue(3));
            return new JsonResult(await _taskRepository.GetTaskAsync(id, userId, groupId));
        }
    }
}