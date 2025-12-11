using LmsApi.Data;
using LmsApi.Models;
using LmsApi.Models.DTOs;
using LmsApi.Models.Enums;
using LmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using LmsApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace LmsApi.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailService _emailService;

        public RegistrationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }
        public async Task<Result<string>?> RegisterAsync(RegisterDto model, string baseUrl)
        {
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
                return Result<string>.Fail("Invalid role");

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Result<string>.Fail(result.Errors.ToString() ?? "Failed to create user.");

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

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{baseUrl}/auth/confirmemail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
                await _emailService.SendEmail(user.Email, "Confirm your Upskeel account", $"Click here to confirm: {confirmationLink}");
                return Result<string>.Ok(confirmationLink);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Result<string>.Ok("User created!");
            }            
        }
    }
}
