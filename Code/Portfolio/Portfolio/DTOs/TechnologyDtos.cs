using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Portfolio.DTOs
{

    public class TechnologyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    // Used internally by ITechnologyService — not bound directly from the HTTP request
    public class CreateTechnologyDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    // This is what the controller action actually binds from [FromForm] —
    // wraps the file as a property so Swashbuckle can generate a schema for it.
    public class CreateTechnologyRequestDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public IFormFile? Icon { get; set; }
    }
}
