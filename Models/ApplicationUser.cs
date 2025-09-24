using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LmsApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public required string FullName { get; set; }
        public bool IsApproved { get; set; } = true;
        
        //Navigation
        public ICollection<Course>? Courses { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<InstructorApprovalRequest>? ApprovalRequests { get; set; }

    }

}
