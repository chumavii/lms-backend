using LmsApi.Data;
using LmsApi.Models;
using LmsApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        

        /// <summary>
        /// Enrolls the currently authenticated user in the specified course.
        /// </summary>
        [HttpPost("enroll/{courseId}/")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> enroll(int courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound(new { message = "Course not found" });

            // Check if already enrolled
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existing != null) return BadRequest(new { message = "You are already enrolled in this course." });

            var enrollment = new Enrollment
            {
                UserId = userId,
                User = user,
                CourseId = courseId,
                Course = course,
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var response = new EnrollmentDto
            {
                UserId = userId,
                FullName = user.FullName,
                CourseId = courseId,
                Title = course.Title,
                Progress = enrollment.Progress,
                CreatedAt = enrollment.CreatedAt
            };

            return CreatedAtAction(nameof(getMyEnrollment), new { id = enrollment.Id }, response);

        }
        

        /// <summary>
        /// Retrieves the list of enrollments for the currently authenticated student.
        /// </summary>
        [HttpGet("myenrollments/")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> getMyEnrollment()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var enrollments = await _context.Enrollments
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(c => c.UserId == userId)
                .Select(c => new EnrollmentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FullName,
                    CourseId = c.CourseId,
                    Title = c.Course.Title,
                    Description = c.Course.Description,
                    Progress = c.Progress,
                    CreatedAt = c.CreatedAt,
                    InstructorName = c.Course.Instructor != null ? c.Course.Instructor.FullName : string.Empty,
                    InstructorEmail = c.Course.Instructor != null ? c.Course.Instructor.Email : string.Empty

                }).ToListAsync();
            return Ok(enrollments);
        }


        /// <summary>
        /// Retrieves a list of enrollments for a specific course.  
        /// </summary>
        /// <remarks>This method is restricted to users with the "Instructor" role. The instructor must
        /// own the specified course; otherwise, the method returns a 404 status code. If the instructor is not
        /// authenticated, the method returns a 401 status code.
        /// </remarks>
        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> getCourseEnrollments(int courseId)
        {
            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (instructorId == null) return Unauthorized();

            // Ensure the course belongs to this instructor
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (course == null) return NotFound("Course not found or you do not own it");

            var enrollments = await _context.Enrollments
                .Include(c => c.User)
                .Where(c => c.CourseId == courseId)
                .Select(c => new EnrollmentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FullName,
                    CourseId = c.CourseId,
                    Title = c.Course.Title,
                    Progress = c.Progress,
                    CreatedAt = c.CreatedAt,
                    InstructorName = c.Course.Instructor != null ? c.Course.Instructor.FullName : string.Empty,
                    InstructorEmail = c.Course.Instructor != null ? c.Course.Instructor.Email : string.Empty
                }).ToListAsync();

            return Ok(enrollments);
        }


        /// <summary>
        /// Retrieves all enrollments in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetAllEnrollments()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course).ThenInclude(c => c.Instructor)
                .Include(e => e.User)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    FullName = e.User.FullName,
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    Progress = e.Progress,
                    CreatedAt = e.CreatedAt,
                    InstructorName = e.Course.Instructor != null ? e.Course.Instructor.FullName : string.Empty,
                    InstructorEmail = e.Course.Instructor != null ? e.Course.Instructor.Email : string.Empty
                })
                .ToListAsync();

            return Ok(enrollments);
        }

    }
}
