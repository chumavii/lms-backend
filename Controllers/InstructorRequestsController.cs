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

        /// <summary>
        /// Retrieves a list of instructor approval requests.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(InstructorRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                    Email = c.Email,
                    Status = c.Status,
                    RequestedAt = c.RequestedAt,
                    ReviewedAt = c.ReviewedAt
                }).ToListAsync();

            if (!approvalRequests.Any())
                return Ok(new List<InstructorRequestDto>());

            return Ok(approvalRequests);
        }


        /// <summary>
        /// Approves an instructor approval request with the specified identifier.
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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


        /// <summary>
        /// Rejects an instructor approval request with the specified identifier.
        /// </summary>
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
