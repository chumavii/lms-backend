using LmsApi.Data;
using LmsApi.Models;
using LmsApi.Models.DTOs;
using LmsApi.Models.Enums;
using LmsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController (ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, TokenService tokenService, EmailService emailService, IConfiguration config) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly TokenService _tokenService = tokenService;
        private readonly EmailService _emailService = emailService;
        private readonly IConfiguration _config = config;

        /// <summary>
        /// Registers a new user with the specified details and assigns the appropriate role.
        /// </summary>
        /// <param name="model">The registration details provided by the user, including email, password, full name, and role.</param>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isInstructor = model.Role == "Instructor";
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                IsApproved = !isInstructor
            };

            //Check role is valid using roles in Db
            if (!await _roleManager.RoleExistsAsync(model.Role))
                return BadRequest("InvalidRole");

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            //User needs to be assigned a role after creation
            if (!await _userManager.IsInRoleAsync(user, model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            if (isInstructor)
            {
                var approvalRequest = new InstructorApprovalRequest
                {
                    User = user,
                    UserId = user.Id,
                    Email = user.Email,
                    Status = ApprovalStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };
                _context.InstructorApprovalRequests.Add(approvalRequest);
                await _context.SaveChangesAsync();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth",
                new { userId = user.Id, token }, Request.Scheme);

            await _emailService.SendEmail(user.Email, "Confirm your Upskeel account", $"Click here to confirm: {confirmationLink}");

            return Ok(new { message = "Registration successful" });
        }

        /// <summary>
        /// Confirms the email address of a user based on the provided user ID and confirmation token.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose email address is being confirmed.</param>
        /// <param name="token">The email confirmation token issued for the user.</param>
        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid User");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? Ok("Email confirmed") : BadRequest("Email confirmation failed");
        }

        /// <summary>
        /// Authenticates a user based on the provided login credentials.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if(!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Instructor") && user.IsApproved == false)
                return Unauthorized("Instructor accounts require admin approval!");

            var token = _tokenService.CreateToken(user, roles);

            return Ok(new { Token = token });
        }

        /// <summary>
        /// Retrieves the currently authenticated user's details.
        /// </summary>
        /// <remarks>This method returns information about the currently authenticated user, including
        /// their full name, email, and roles.  The user must be authenticated to access this endpoint.</remarks>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            };

            return Ok(userDto);
        }

        /// <summary>
        /// Initiates the password reset process by generating a reset token and sending a reset link to the user's
        /// email address.
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("Invalid Email");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = _config["FrontEnd:Url"];
            var resetLink = $"{frontendUrl}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailService.SendEmail(user.Email ?? model.Email, "Reset your LMS password", $"Click here to reset: {resetLink}");

            return Ok("Password reset link has been sent to your email.");
        }

        /// <summary>
        /// Resets the password for a user based on the provided token and new password.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return BadRequest("Invalid User");

            var decodedToken = Uri.UnescapeDataString(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            return result.Succeeded ? Ok("Password reset successful") : BadRequest("Password reset failed");
        }



        /// <summary>
        /// Retrieves a list of all users along with their roles and other details.
        /// </summary>
        /// <remarks>This method is accessible only to users with the "Admin" role. It returns a
        /// collection of user details, including their ID, full name, email, approval status, and assigned roles. If
        /// the current user is not authenticated or does not have the "Admin" role, the method will return an
        /// appropriate HTTP status code (e.g., 401 Unauthorized or 403 Forbidden).</remarks>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var currentRoles = await _userManager.GetRolesAsync(currentUser);
            if (!currentRoles.Contains("Admin"))
            {
                return Forbid();
            }

            var users = await _context.Users.ToListAsync();
            var usersList = new List<UsersDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersList.Add(new UsersDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    IsApproved = user.IsApproved,
                    Roles = roles.ToList()
                });
            }
            return Ok(usersList);
        }
    }
}
