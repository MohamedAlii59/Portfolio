using System.ComponentModel.DataAnnotations;

namespace Portfolio.DTOs
{

    public class TechnologyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    public class CreateTechnologyDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        // Icon file comes in separately as IFormFile in the service method, not through this DTO
    }
}
