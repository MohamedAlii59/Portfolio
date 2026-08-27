namespace Portfolio.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }

        public DateTime? ProjectDate { get; set; }

        // External link only — never uploaded/hosted directly
        public string? DemoVideoUrl { get; set; }

        public string? GithubUrl { get; set; }   // project repo link
        public string? ProjectUrl { get; set; }  // live demo link

        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
        public ICollection<ProjectTechnology> ProjectTechnologies { get; set; } = new List<ProjectTechnology>();
    }
}
