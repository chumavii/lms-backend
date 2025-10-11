using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        public required string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public required string UserId { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

}
