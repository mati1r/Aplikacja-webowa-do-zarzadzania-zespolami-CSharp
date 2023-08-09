using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Calendar> Calendar { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
    }
}
