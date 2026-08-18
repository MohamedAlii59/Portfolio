using System.ComponentModel.DataAnnotations;

namespace Portfolio.DTOs
{
    public class WorkExperienceDto
    {
        public int Id { get; set; }
        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpsertWorkExperienceDto
    {
        [Required, MaxLength(200)]
        public string Company { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Role { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
