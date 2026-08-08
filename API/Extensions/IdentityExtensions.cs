using API.Infrastructure.Persistence.Context;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;


namespace API.Extensions
{
    /// <summary>
    /// Extension methods for configuring Identity services.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Adds Identity with Entity Framework stores and configures password policies.
        /// </summary>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}