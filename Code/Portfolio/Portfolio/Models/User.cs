namespace Portfolio.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Public portfolio URL segment, e.g. yoursite.com/u/{Slug}
        public string Slug { get; set; } = string.Empty;

        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? GithubUrl { get; set; }
        public string? LinkedInUrl { get; set; }

        // Resume: one file per user, nullable = no resume uploaded
        public string? ResumeUrl { get; set; }
        public string? ResumeFileName { get; set; }

        // Forces a password change on first login since you create the account manually
        public bool MustChangePasswordOnFirstLogin { get; set; } = true;

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Education> Education { get; set; } = new List<Education>();
        public ICollection<WorkExperience> WorkExperience { get; set; } = new List<WorkExperience>();
        public ICollection<UserTechnology> UserTechnologies { get; set; } = new List<UserTechnology>();
    }
}
