namespace LmsApi.Models
{
    public class Enrollment
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }

        public required int CourseId { get; set; }
        public required Course Course { get; set; }

        public int Progress { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
