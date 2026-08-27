namespace Portfolio.Models
{
    public class Education
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string Institution { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public string? FieldOfStudy { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // null = "present"

        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
