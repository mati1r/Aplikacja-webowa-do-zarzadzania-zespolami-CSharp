using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;
using Aplikacja_webowa_do_zarządzania_zespołami.Models;
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
    }
}
