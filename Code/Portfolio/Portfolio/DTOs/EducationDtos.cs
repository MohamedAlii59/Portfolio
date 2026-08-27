using System.ComponentModel.DataAnnotations;
namespace Portfolio.DTOs
{


    public class EducationDto
    {
        public int Id { get; set; }
        public string Institution { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public string? FieldOfStudy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpsertEducationDto
    {
        [Required, MaxLength(200)]
        public string Institution { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Degree { get; set; }

        [MaxLength(150)]
        public string? FieldOfStudy { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; } // null = "present"

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
