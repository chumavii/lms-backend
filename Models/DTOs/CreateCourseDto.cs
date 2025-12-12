using LmsApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class CreateCourseDto
    {
        [Required]
        public required string Title { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public bool IsDraft { get; set; } 
    }
}