using Aplikacja_webowa_do_zarządzania_zespołami.PartialModels;
using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.DAO
{
    public class TaskDAO : ITaskDAO
    {
        private readonly DatabaseContext _dbContext;
        public TaskDAO(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static string GetColor(string status)
        {
            if(status == "nieukończone")
            {
                return "#375a7f";
            }
            else if(status == "w trakcie")
            {
                return "#f39c12";
            }
            else
            {
                return "#00bc8c";
            }
        }

        //Calendar
        public Task<List<CalendarEventsDTO>> GetCalendarEventsAsync(int? userId, int? groupId)
        {
            return _dbContext.Tasks
                .Where(t => t.groups_group_id == groupId && t.users_user_id == userId)
                .Select(t => new CalendarEventsDTO
                {
                    taskId = t.task_id,
                    title = t.task_name,
                    start = t.start_date.ToString("yyyy-MM-dd"),
                    end = t.end_date.ToString("yyyy-MM-dd"),
                    color = GetColor(t.status)
                })
                .ToListAsync();
        }

        public void CompleteTask(int taskId, string feedbackMessage)
        {
            var task = _dbContext.Tasks.Where(t => t.task_id == taskId).First();
            task.status = "ukończone";
            task.feedback = feedbackMessage;
            task.finish_date = DateTime.Now.AddSeconds(-DateTime.Now.Second);
            _dbContext.Tasks.Update(task);
            _dbContext.SaveChanges();
        }
        
        public void CurrentTask(int taskId, string feedbackMessage)
        {
            var task = _dbContext.Tasks.Where(t => t.task_id == taskId).First();
            task.status = "w trakcie";
            _dbContext.Tasks.Update(task);
            _dbContext.SaveChanges();
        }

        public void NotCompleteTask(int taskId, string feedbackMessage)
        {
            var task = _dbContext.Tasks.Where(t => t.task_id == taskId).First();
            task.status = "nieukończone";
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
            return _dbContext.Tasks.Any(t => t.task_id == taskId && t.groups_group_id == groupId && t.users_user_id == userId && t.status != "ukończone");
        }

        public void CreateTask(Models.Task task, int groupId)
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
        }

        public void DeleteTask(Models.Task task)
        {
            _dbContext.Remove(task);
            _dbContext.SaveChanges();
        }

        public void EditTask(Models.Task task)
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
        }

        public bool IsTaskCompleted(Models.Task task)
        {
            return _dbContext.Tasks.Any(t => t.task_id == task.task_id && t.status == "ukończone");
        }

        public bool IsExistingTaskInGroup(Models.Task task, int? groupId)
        {
            return _dbContext.Tasks.Any(t => t.task_id == task.task_id && t.groups_group_id == groupId);
        }

        public List<OwnerTaskPartial> GetAllTaskForGroup(int? groupId)
        {
            return _dbContext.Tasks
                .Where(t => t.groups_group_id == groupId)
                .Select(t => new OwnerTaskPartial
                {
                    users_user_id = t.users_user_id,
                    groups_group_id = t.groups_group_id,
                    task_id = t.task_id,
                    task_name = t.task_name,
                    description = t.description,
                    start_date = t.start_date,
                    end_date = t.end_date,
                    finish_date = t.finish_date,
                    priority = t.priority,
                    status = t.status,
                    feedback = t.feedback,
                    username = _dbContext.Users_Groups
                        .Where(ug => ug.users_user_id == t.users_user_id && ug.groups_group_id == groupId)
                        .Any()
                        ? t.Users.username
                        : "Brak"
                })
                .ToList();
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
