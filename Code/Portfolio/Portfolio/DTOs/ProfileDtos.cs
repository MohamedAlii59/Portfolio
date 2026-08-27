using System.ComponentModel.DataAnnotations;
using Portfolio.Validation;
namespace Portfolio.DTOs
{

    // What the frontend receives when viewing a profile — used both publicly
    // (visitor viewing the portfolio by slug) and privately (client viewing "me").
    public class ProfileResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? GithubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? ResumeFileName { get; set; }
        public bool HasResume { get; set; }
    }

    // What the client submits when editing their profile.
    // Note: no Email or Password fields here — those are handled through
    // AuthController (change-password), never through the profile update endpoint.
    public class UpdateProfileRequestDto
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Bio { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [OptionalUrl]
        public string? GithubUrl { get; set; }

        [OptionalUrl]
        public string? LinkedInUrl { get; set; }

        [Required, RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
        public string Slug { get; set; } = string.Empty;
    }
    public class UploadPhotoRequestDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class UploadResumeRequestDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
