using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace Infrastructure.Persistence.Seed
{
    public static class DefaultAdminSeeder
    {
        public static async Task SeedAppUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Seed roles
            await DefaultRolesSeeder.SeedRolesAsync(roleManager);

            // 2. Create Admin user if no users exist
            if (!userManager.Users.Any())
            {
                var user = new ApplicationUser
                {
                    UserName = "gaberemadbader@gmail.com",
                    PhoneNumber = "01019806684",
                    FullName = "Gaber Emad Badr",
                    Email = "gaberemadbader@gmail.com",
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(user, "Admin@123");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        System.Console.WriteLine($"Error: {error.Code} - {error.Description}");
                    }
                }
            }
        }
    }
}
