namespace LmsApi.Models.DTOs
{
    public class ReassignCourseResponseDto
    {
        public int CourseId { get; set; }
        public required string InstructorId { get; set; }
        public required string InstructorName { get; set; }
        public string Message { get; set; } = "Instructor reassigned successfully";
    }
}
