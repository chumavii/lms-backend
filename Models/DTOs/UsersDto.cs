using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models.DTOs
{
    public class UsersDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public bool IsApproved { get; set; }
        public List<string>? Roles { get; set; }
    }
}
