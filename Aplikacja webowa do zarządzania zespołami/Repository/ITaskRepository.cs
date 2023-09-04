using Aplikacja_webowa_do_zarządzania_zespołami.DTO_models_and_static_vars;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public interface ITaskRepository
    {
        Task<List<CalendarEventsDTO>> GetCalendarEventsAsync(int? userId, int? groupId);
    }
}
