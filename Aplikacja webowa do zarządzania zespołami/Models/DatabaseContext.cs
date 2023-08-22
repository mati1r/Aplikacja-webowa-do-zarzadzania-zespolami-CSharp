using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Calendar> Calendar { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User_Group> Users_Groups { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User_Group>()
            .HasKey(pk => new { pk.users_user_id, pk.groups_group_id });

            modelBuilder.Entity<User_Group>()
                .HasOne(pk => pk.Users)
                .WithMany(p => p.Users_Groups)
                .HasForeignKey(pk => pk.users_user_id);

            modelBuilder.Entity<User_Group>()
                .HasOne(pk => pk.Groups)
                .WithMany(k => k.Users_Groups)
                .HasForeignKey(pk => pk.groups_group_id);

            modelBuilder.Entity<Models.Task>()
                .HasKey(pk => new { pk.task_id });

            modelBuilder.Entity<Models.Task>()
                .HasOne(pk => pk.Users)
                .WithMany(p => p.Tasks)
                .HasForeignKey(pk => pk.users_user_id);

            modelBuilder.Entity<Models.Task>()
                .HasOne(pk => pk.Groups)
                .WithMany(k => k.Tasks)
                .HasForeignKey(pk => pk.groups_group_id);
        }
    }
}
