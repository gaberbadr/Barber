using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationServices(
            this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
                .AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"))
                .AddPolicy("UserPolicy", policy =>
                    policy.RequireRole("User", "Admin"))
                .AddPolicy("BarberPolicy", policy =>
                    policy.RequireRole("Barber"));

            return services;
        }
    }
}

//[Authorize(Policy = "AdminPolicy")] for admin access only
//[Authorize(Policy = "UserPolicy")] for admin and user access 
//[Authorize(Policy = "BarberPolicy")] for barber access only
