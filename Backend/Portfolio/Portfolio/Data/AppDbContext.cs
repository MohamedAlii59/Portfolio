using Microsoft.EntityFrameworkCore;
using Portfolio.Models;
using Portfolio.Services.Interfaces;

namespace Portfolio.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser)
            : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
        public DbSet<Technology> Technologies => Set<Technology>();
        public DbSet<UserTechnology> UserTechnologies => Set<UserTechnology>();
        public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
        public DbSet<Education> EducationEntries => Set<Education>();
        public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Slug).IsUnique();

            modelBuilder.Entity<UserTechnology>().HasKey(ut => new { ut.UserId, ut.TechnologyId });
            modelBuilder.Entity<ProjectTechnology>().HasKey(pt => new { pt.ProjectId, pt.TechnologyId });

            modelBuilder.Entity<Project>()
                .HasOne(p => p.User).WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectImage>()
                .HasOne(pi => pi.Project).WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProjectId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Education>()
                .HasOne(e => e.User).WithMany(u => u.Education)
                .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkExperience>()
                .HasOne(w => w.User).WithMany(u => u.WorkExperience)
                .HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);

            // Multi-tenant safety net: every query on these tables is automatically
            // scoped to the logged-in user, even if a service forgets to filter manually.
            modelBuilder.Entity<Project>().HasQueryFilter(p => _currentUser.UserId == null || p.UserId == _currentUser.UserId);
            modelBuilder.Entity<Education>().HasQueryFilter(e => _currentUser.UserId == null || e.UserId == _currentUser.UserId);
            modelBuilder.Entity<WorkExperience>().HasQueryFilter(w => _currentUser.UserId == null || w.UserId == _currentUser.UserId);
            modelBuilder.Entity<UserTechnology>().HasQueryFilter(ut => _currentUser.UserId == null || ut.UserId == _currentUser.UserId);
        }
    }
}

