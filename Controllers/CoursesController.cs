using System.Security.Claims;
using LmsApi.Data;
using LmsApi.Models;
using LmsApi.Models.DTOs;
using LmsApi.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        /// <summary>
        /// Retrieves a list of all published courses.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CourseListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.Status == CourseStatus.Published)
                .Select(c => new CourseListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : null,
                    InstructorEmail = c.Instructor != null ? c.Instructor.Email : null,
                    Status = c.Status
                }).ToListAsync();

            return Ok(courses);
        }

        /// <summary>
        /// Retrieves a list of all draft courses.
        /// </summary>
        /// <returns></returns>
        [HttpGet("draft")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CourseListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetDraftCourses()
        {
            var draftCourses = await _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.Status == CourseStatus.Draft)
                .Select(c => new CourseListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : null,
                    InstructorEmail = c.Instructor != null ? c.Instructor.Email : null,
                    Status = c.Status
                }).ToListAsync();

            return Ok(draftCourses);
        }

        /// <summary>
        /// Retrieves the details of a course by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CourseListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseListDto>> GetCourse(int id)
        {
            var course = await _context.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) 
                return NotFound();

            var courseDto = new CourseListDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                InstructorName = course.Instructor?.FullName,
                InstructorEmail = course.Instructor?.Email,
                Status = course.Status
            };
            return Ok(courseDto);
        }


        /// <summary>
        /// Creates a new course for the authenticated instructor.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(CreateCourseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
                return Unauthorized();

            var courseStatus = model.IsDraft ? CourseStatus.Draft : CourseStatus.Published;

            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                InstructorId = userId,
                Status = courseStatus
            };
            
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var instructor = await _context.Users.FindAsync(userId);
            var response = new CreateCourseResponseDto
            {
                Title = model.Title,
                Description = model.Description,
                InstructorName = instructor?.FullName ?? string.Empty,
                InstructorEmail = instructor?.Email ?? string.Empty,
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, response);
        }


        /// <summary>
        /// Publishes the course with the specified identifier.
        /// </summary>
        [HttpPut("publish/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PublishCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound(new { message = "Course not found!" });

            course.Status = CourseStatus.Published;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course published successfully" });
        }

        /// <summary>
        /// Retrieves all draft courses created by the authenticated instructor.
        /// </summary>
        /// <remarks>This operation is restricted to users with the "Instructor" role.
        /// Returns only courses with Draft status created by this instructor.</remarks>
        [HttpGet("my-drafts")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetMyDraftCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var draftCourses = await _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == userId && c.Status == CourseStatus.Draft)
                .Select(c => new CourseListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : null,
                    InstructorEmail = c.Instructor != null ? c.Instructor.Email : null,
                    Status = c.Status
                }).ToListAsync();

            return Ok(draftCourses);
        }

        /// <summary>
        /// Retrieves all courses created by the authenticated instructor.
        /// </summary>
        /// <remarks>This operation is restricted to users with the "Instructor" role.
        /// Returns all courses (both Published and Draft) created by the instructor.</remarks>
        [HttpGet("my")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetMyInstructorCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == userId)
                .Select(c => new CourseListDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : null,
                    InstructorEmail = c.Instructor != null ? c.Instructor.Email : null,
                    Status = c.Status
                }).ToListAsync();

            return Ok(courses);
        }

        /// <summary>
        /// Reassigns a course to a new instructor.
        /// </summary>
        /// <remarks>This operation is restricted to users with the "Admin" role.  The method validates
        /// that the specified instructor exists and is assigned the "Instructor" role  before reassigning the
        /// course.
        /// </remarks>
        [HttpPut("reassign/")]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ReassignCourseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReassignCourse(ReassignCourseDto model)
        {
            var course = await _context.Courses.FindAsync(model.CourseId);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            var newInstructor = await _context.Users.FindAsync(model.InstructorId);
            if (newInstructor == null)
                return NotFound(new { message = "Instructor not found" });

            var isInstructor = await _userManager.IsInRoleAsync(newInstructor, "Instructor");
            if (!isInstructor)
                return BadRequest(new { message = "User is not an Instructor" });

            course.InstructorId = newInstructor.Id;
            course.Instructor = newInstructor;
            await _context.SaveChangesAsync();

            var response = new ReassignCourseResponseDto
            {
                CourseId = course.Id,
                InstructorId = newInstructor.Id,
                InstructorName = newInstructor.FullName
            };

            return Ok(response);
        }


        /// <summary>
        /// Updates the details of an existing course.
        /// </summary>
        /// <remarks>This operation requires the caller to be authenticated and authorized with the
        /// "Admin" or "Instructor" role.
        /// </remarks>
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCourse(int id, CreateCourseDto updated)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) 
                return NotFound();

            course.Title = updated.Title;
            course.Description = updated.Description;
            await _context.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Deletes the course with the specified identifier.
        /// </summary>
        /// <remarks>This action requires the caller to have the "Admin" role. The course is permanently
        /// removed  from the database if it exists.
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) 
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
