using Microsoft.EntityFrameworkCore;

namespace Aplikacja_webowa_do_zarządzania_zespołami.Models
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Calendar> Calendar { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Groups> Groups { get; set; }
        public DbSet<Users_Groups> Users_Groups { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users_Groups>()
            .HasKey(pk => new { pk.users_user_id, pk.groups_group_id });

            modelBuilder.Entity<Users_Groups>()
                .HasOne(pk => pk.Users)
                .WithMany(p => p.Users_Groups)
                .HasForeignKey(pk => pk.users_user_id);

            modelBuilder.Entity<Users_Groups>()
                .HasOne(pk => pk.Groups)
                .WithMany(k => k.Users_Groups)
                .HasForeignKey(pk => pk.groups_group_id);

            modelBuilder.Entity<Tasks>()
                .HasKey(pk => new { pk.task_id });

            modelBuilder.Entity<Tasks>()
                .HasOne(pk => pk.Users)
                .WithMany(p => p.Tasks)
                .HasForeignKey(pk => pk.users_user_id);

            modelBuilder.Entity<Tasks>()
                .HasOne(pk => pk.Groups)
                .WithMany(k => k.Tasks)
                .HasForeignKey(pk => pk.groups_group_id);
        }
    }
}
