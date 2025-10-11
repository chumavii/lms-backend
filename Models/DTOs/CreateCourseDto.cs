using LmsApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class CreateCourseDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}