namespace DisasterManagementApp.Data
{
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Incident> Incidents { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between Incident and User
            modelBuilder.Entity<Incident>()
                .HasOne(i => i.User) // Navigation property in Incident
                .WithMany() // Assuming User has a collection of Incidents
                .HasForeignKey(i => i.UserId) // Foreign key in Incident
                .OnDelete(DeleteBehavior.Cascade); // Optional: specify delete behavior

            // Seed default roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "user" }, // Default "user" role
                new Role { RoleId = 2, Name = "admin" }
            );
        }
    }
}
