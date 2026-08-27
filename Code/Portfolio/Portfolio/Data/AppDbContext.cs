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
            var currentUserId = _currentUser.UserId;
            modelBuilder.Entity<Project>().HasQueryFilter(p => currentUserId == null || p.UserId == currentUserId);
            modelBuilder.Entity<Education>().HasQueryFilter(e => currentUserId == null || e.UserId == currentUserId);
            modelBuilder.Entity<WorkExperience>().HasQueryFilter(w => currentUserId == null || w.UserId == currentUserId);
            modelBuilder.Entity<UserTechnology>().HasQueryFilter(ut => currentUserId == null || ut.UserId == currentUserId);

            // Postgres requires DateTime values to be UTC ("Kind = Utc"). Rather than
            // remembering to call DateTime.SpecifyKind(...) everywhere a date is set,
            // this converter automatically treats every DateTime read from/written to
            // the database as UTC across all entities, once, in one place.
            var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableUtcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(utcConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(nullableUtcConverter);
                }
            }
        }
    }
}

