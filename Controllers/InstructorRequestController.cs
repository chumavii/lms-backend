using LmsApi.Data;
using LmsApi.Models;
using LmsApi.Models.DTOs;
using LmsApi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<InstructorRequestDto>>> GetApprovalRequests()
        {
            var approvalRequests = await _context.InstructorApprovalRequests
                .Include(c => c.User)
                //.Where(c => c.Status.Equals(ApprovalStatus.Pending))
                .Select(c => new InstructorRequestDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FullName,
                    Status = c.Status,
                    RequestedAt = c.RequestedAt,
                    ReviewedAt = c.ReviewedAt
                }).ToListAsync();

            if (!approvalRequests.Any())
                return Ok(new List<InstructorRequestDto>());

            return Ok(approvalRequests);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveRequest(Guid id)
        {
            var request = await _context.InstructorApprovalRequests
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (request == null) return NotFound();

            request.Status = ApprovalStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.User.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Request approved!" });
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RejectRequest(Guid id)
        {
            var request = await _context.InstructorApprovalRequests
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (request == null) return NotFound();

            request.Status = ApprovalStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.User.IsApproved = false;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Request rejected!" });
        }
    }
}
