using System.ComponentModel.DataAnnotations;

namespace Portfolio.DTOs
{

    public class ProjectImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public DateTime? ProjectDate { get; set; }
        public string? DemoVideoUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? ProjectUrl { get; set; }
        public int DisplayOrder { get; set; }
        public List<ProjectImageDto> Images { get; set; } = new();
        public List<TechnologyDto> Technologies { get; set; } = new();
    }

    public class UpsertProjectDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? ShortDescription { get; set; }

        public DateTime? ProjectDate { get; set; }

        [Url]
        public string? DemoVideoUrl { get; set; }

        [Url]
        public string? GithubUrl { get; set; }

        [Url]
        public string? ProjectUrl { get; set; }

        // Ids of technologies (from the shared Technology list) used in this project
        public List<int> TechnologyIds { get; set; } = new();
    }

    public class ReorderRequestDto
    {
        [Required]
        public List<int> OrderedIds { get; set; } = new();
    }
}
