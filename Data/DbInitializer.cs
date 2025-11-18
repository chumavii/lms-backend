using LmsApi.Models;
using Microsoft.AspNetCore.Identity;

namespace LmsApi.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<IdentityRole>>();

            string[] roles = { "Admin", "Instructor", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Admin",
                    IsApproved = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Admin");
                else
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
