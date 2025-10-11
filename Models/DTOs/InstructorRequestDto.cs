using LmsApi.Models.Enums;

namespace LmsApi.Models.DTOs
{
    public class InstructorRequestDto
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public string? FullName { get; set; }
        public ApprovalStatus Status { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
