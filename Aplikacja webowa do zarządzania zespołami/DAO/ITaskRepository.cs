using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Microsoft.AspNetCore.Mvc;

namespace Aplikacja_webowa_do_zarządzania_zespołami.DAO
{
    public interface ITaskDAO
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
        void CreateTask(Models.Task task, int groupId);
        void DeleteTask(Models.Task task);
        void EditTask(Models.Task task);
        bool IsTaskCompleted(Models.Task task);
        bool IsExistingTaskInGroup(Models.Task task, int? groupId);
        List<OwnerTaskPartial> GetAllTaskForGroup(int? groupId);
        Task<Models.Task> GetTaskAsync(int id, int? groupId);

    }
}
