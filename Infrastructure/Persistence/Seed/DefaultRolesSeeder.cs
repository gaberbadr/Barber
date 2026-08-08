using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seed
{
    public static class DefaultRolesSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User", "Barber" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
