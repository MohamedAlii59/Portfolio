namespace Portfolio.Models
{
    public class Technology
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }

        public ICollection<UserTechnology> UserTechnologies { get; set; } = new List<UserTechnology>();
        public ICollection<ProjectTechnology> ProjectTechnologies { get; set; } = new List<ProjectTechnology>();
    }

    // Join table: technologies attached to the profile ("my skills")
    public class UserTechnology
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int TechnologyId { get; set; }
        public Technology? Technology { get; set; }
    }

    // Join table: technologies attached to a specific project
    public class ProjectTechnology
    {
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int TechnologyId { get; set; }
        public Technology? Technology { get; set; }
    }
}
