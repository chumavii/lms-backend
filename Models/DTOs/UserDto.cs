namespace LmsApi.Models.DTOs
{
    public class UserDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required List<string> Roles { get; set; }
    }

}
