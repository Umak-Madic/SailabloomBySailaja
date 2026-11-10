using Microsoft.AspNetCore.Identity;
using Sailabloom.Infrastructure.Data;

namespace Sailabloom.Web.Data;

/// <summary>
/// Creates the "Admin" role and the owner account from configuration
/// (AdminUser:Email / AdminUser:Password). The appsettings values are
/// CHANGE-ME placeholders — override them with user secrets or environment
/// variables (AdminUser__Email / AdminUser__Password) and never commit a
/// real password. See README.md.
/// </summary>
public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var email = configuration["AdminUser:Email"];
        var password = configuration["AdminUser:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Store Owner",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AdminRole);
            }
        }
    }
}
