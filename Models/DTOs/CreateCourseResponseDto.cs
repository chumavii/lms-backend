using LmsApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class CreateCourseResponseDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? InstructorName { get; set; }
        public string? InstructorEmail { get; set; }
        public CourseStatus Status { get; set; }
    }
}