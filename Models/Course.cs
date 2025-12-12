using LmsApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models
{
    public class Course
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? InstructorId { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public ApplicationUser? Instructor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<Module>? Modules { get; set; }
    }
}
