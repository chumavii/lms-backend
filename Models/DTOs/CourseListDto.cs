using LmsApi.Models.Enums;

namespace LmsApi.Models.DTOs
{
    public class CourseListDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public CourseStatus Status { get; set; }
        public string? InstructorName { get; set; }
        public string? InstructorEmail { get; set; }
    }   
}
