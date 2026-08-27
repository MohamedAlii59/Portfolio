namespace Portfolio.Models
{
    public class WorkExperience
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // null = "present"

        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}

