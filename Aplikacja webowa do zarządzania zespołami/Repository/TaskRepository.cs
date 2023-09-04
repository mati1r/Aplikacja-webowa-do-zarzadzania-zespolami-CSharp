using Aplikacja_webowa_do_zarządzania_zespołami.Models;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DatabaseContext _dbContext;
        public TaskRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
