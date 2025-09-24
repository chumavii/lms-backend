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

namespace LmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController (ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, TokenService tokenService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly TokenService _tokenService = tokenService;

        [HttpPost("register")]
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
                    Status = ApprovalStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };
                _context.InstructorApprovalRequests.Add(approvalRequest);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
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

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
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
