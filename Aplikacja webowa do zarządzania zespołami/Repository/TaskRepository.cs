using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DatabaseContext _dbContext;
        public TaskRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Calendar
        public Task<List<CalendarEventsDTO>> GetCalendarEventsAsync(int? userId, int? groupId)
        {
            return _dbContext.Tasks
                .Where(t => t.groups_group_id == groupId && t.users_user_id == userId)
                .Select(t => new CalendarEventsDTO
                {
                    title = t.task_name,
                    start = t.start_date.ToString("yyyy-MM-dd"),
                    end = t.end_date.ToString("yyyy-MM-dd")
                })
                .ToListAsync();
        }

        public void CompleteTask(int taskId, string feedbackMessage)
        {
            var task = _dbContext.Tasks.Where(t => t.task_id == taskId).First();
            task.status = "ukończone";
            task.feedback = feedbackMessage;
            task.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second); ;
            _dbContext.Tasks.Update(task);
            _dbContext.SaveChanges();
        }

        public async Task<Models.Task> GetTaskAsync(int id, int? userId, int? groupId)
        {
            //Check if user didn't changed id to an id out of his scope
            if (_dbContext.Tasks.Any(t => t.task_id == id && t.groups_group_id == groupId && t.users_user_id == userId))
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

        public List<Models.Task> GetTasksForUser(int? userId, int? groupId)
        {
            return _dbContext.Tasks
                .Where(t => t.groups_group_id == groupId && t.users_user_id == userId && (t.finish_date > DateTime.Now.AddDays(-7) || t.finish_date == null))
                .ToList();
        }

        public bool IsTaskForUserNotComplete(int taskId, int? userId, int? groupId)
        {
            return _dbContext.Tasks.Any(t => t.task_id == taskId && t.groups_group_id == groupId && t.users_user_id == userId && t.status == "nieukończone");
        }

        public JsonResult CreateTask(Models.Task task, int groupId)
        {
            //Check if task is set to complete if so set finish_date
            if (task.status == "ukończone")
            {
                //Setting a date on now and setting seconds to 0
                task.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            }

            task.groups_group_id = groupId;
            _dbContext.Tasks.Add(task);
            _dbContext.SaveChanges();
            return new JsonResult("success");
        }

        public JsonResult DeleteTask(Models.Task task)
        {
            _dbContext.Remove(task);
            _dbContext.SaveChanges();
            return new JsonResult("success");
        }

        public JsonResult EditTask(Models.Task task)
        {
            //Now lets get original task and change values aside from task_id, group_id, user_id, and feedback
            //if status changed from nieukończone to ukończone then add finish_date
            Models.Task originalTask = _dbContext.Tasks.Where(t => t.task_id == task.task_id).First();
            if (originalTask.status == "nieukończone" && task.status == "ukończone")
            {
                //Setting a date on now and setting seconds to 0
                originalTask.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            }
            originalTask.task_name = task.task_name;
            originalTask.users_user_id = task.users_user_id;
            originalTask.description = task.description;
            originalTask.start_date = task.start_date;
            originalTask.end_date = task.end_date;
            originalTask.status = task.status;
            originalTask.priority = task.priority;
            _dbContext.Update(originalTask);
            _dbContext.SaveChanges();
            return new JsonResult("success");
        }

        public bool IsTaskCompleted(Models.Task task)
        {
            return _dbContext.Tasks.Any(t => t.task_id == task.task_id && t.status == "ukończone");
        }

        public bool IsExistingTaskInGroup(Models.Task task, int? groupId)
        {
            return _dbContext.Tasks.Any(t => t.task_id == task.task_id && t.groups_group_id == groupId);
        }

        public List<Models.Task> GetAllTaskForGroup(int? groupId)
        {
            return _dbContext.Tasks.Where(t => t.groups_group_id == groupId).ToList();
        }

        public async Task<Models.Task> GetTaskAsync(int id, int? groupId)
        {
            //Check if requested task is in the same group as owner that is loged in
            if (_dbContext.Tasks.Any(t => t.task_id == id && t.groups_group_id == groupId))
            {
                return await _dbContext.Tasks.FirstAsync(t => t.task_id == id);
            }
            else
            {
                Models.Task emptyTask = new Models.Task();
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
    }
}
