using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface ITaskRepository
    {
        //Calendar
        Task<List<CalendarEventsDTO>> GetCalendarEventsAsync(int? userId, int? groupId);
        
        //User Task oraz Owner Task
        bool IsTaskForUserNotComplete(int taskId, int? userId, int? groupId);
        void CompleteTask(int taskId, string feedbackMessage);
        void CurrentTask(int taskId, string feedbackMessage);
        void NotCompleteTask(int taskId, string feedbackMessage);
        List<Models.Task> GetTasksForUser(int? userId, int? groupId);
        Task<Models.Task> GetTaskAsync(int id, int? userId, int? groupId);

        //Owner Task
        JsonResult CreateTask(Models.Task task, int groupId);
        JsonResult DeleteTask(Models.Task task);
        JsonResult EditTask(Models.Task task);
        bool IsTaskCompleted(Models.Task task);
        bool IsExistingTaskInGroup(Models.Task task, int? groupId);
        List<OwnerTaskDTO> GetAllTaskForGroup(int? groupId);
        Task<Models.Task> GetTaskAsync(int id, int? groupId);

    }
}
