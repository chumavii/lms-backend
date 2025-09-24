using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class ReassignCourseDto
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public required string InstructorId { get; set; }
    }
}
