using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage ="Email is required")]
        public required string Email { get; set; }
        [Required(ErrorMessage ="Full name is required")]
        public required string FullName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string Role { get; set; }
    }
}
