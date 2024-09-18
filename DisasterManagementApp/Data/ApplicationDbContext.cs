namespace DisasterManagementApp.Data
{
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Incident> Incidents { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Alert> Alerts { get; set; }
    }
}
