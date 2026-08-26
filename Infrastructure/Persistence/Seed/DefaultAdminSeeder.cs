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

            // 2. Seed default admins
            var admins = new[]
            {
                new
                {
                    UserName = "gaberemadbader@gmail.com",
                    Email = "gaberemadbader@gmail.com",
                    PhoneNumber = "01019806684",
                    FullName = "Gaber Emad Badr",
                    Password = "Admin@123"
                },
                new
                {
                    UserName = "ahmedmegahed580@gmail.com",
                    Email = "ahmedmegahed580@gmail.com",
                    PhoneNumber = "01000000000",
                    FullName = "Ahmed Megahed",
                    Password = "Admin@123"
                },
                new
                {
                    UserName = "ahmedmegaed43@gmail.com",
                    Email = "ahmedmegaed43@gmail.com",
                    PhoneNumber = "01000000000",
                    FullName = "Ahmed Megahed 2",
                    Password = "Admin@123"
                },
                new
                {
                    UserName = "abderhmanelgohary8@gmail.com",
                    Email = "abderhmanelgohary8@gmail.com",
                    PhoneNumber = "01000000000",
                    FullName = "Abderhman Elgohary",
                    Password = "Admin@123"
                }

            };

            foreach (var admin in admins)
            {
                // Check by email so the seeder doesn't create duplicates
                var existingUser = await userManager.FindByEmailAsync(admin.Email);

                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = admin.UserName,
                        PhoneNumber = admin.PhoneNumber,
                        FullName = admin.FullName,
                        Email = admin.Email,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, admin.Password);

                    if (createResult.Succeeded)
                    {
                        if (admin.Email == "gaberemadbader@gmail.com" || admin.Email == "ahmedmegahed580@gmail.com")
                        {
                            await userManager.AddToRoleAsync(user, "Spector");
                        }
                        else
                        {
                            await userManager.AddToRoleAsync(user, "Admin");
                        }
                    }
                    else
                    {
                        foreach (var error in createResult.Errors)
                        {
                            System.Console.WriteLine(
                                $"Error creating {admin.Email}: {error.Code} - {error.Description}"
                            );
                        }
                    }
                }
            }
        }
    }
}

