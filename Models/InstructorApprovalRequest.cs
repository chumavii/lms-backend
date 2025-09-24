using LmsApi.Models.Enums;

namespace LmsApi.Models
{
    public class InstructorApprovalRequest
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}
